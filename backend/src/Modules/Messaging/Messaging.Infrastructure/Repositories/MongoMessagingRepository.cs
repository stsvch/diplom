using Messaging.Application.Interfaces;
using Messaging.Domain.Documents;
using Messaging.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Messaging.Infrastructure.Repositories;

public class MongoMessagingRepository : IMessagingRepository
{
    private readonly IMongoCollection<ChatDocument> _chats;
    private readonly IMongoCollection<MessageDocument> _messages;
    private readonly IMongoCollection<ReadReceiptDocument> _receipts;

    public MongoMessagingRepository(IMongoDatabase database)
    {
        _chats = database.GetCollection<ChatDocument>("chats");
        _messages = database.GetCollection<MessageDocument>("messages");
        _receipts = database.GetCollection<ReadReceiptDocument>("read_receipts");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var chatIndex = Builders<ChatDocument>.IndexKeys.Ascending(c => c.ParticipantIds);
        _chats.Indexes.CreateOne(new CreateIndexModel<ChatDocument>(chatIndex));

        // Unique partial index: at most one CourseChat per CourseId
        var courseIdIndex = Builders<ChatDocument>.IndexKeys.Ascending(c => c.CourseId);
        var courseChatFilter = Builders<ChatDocument>.Filter.Eq(c => c.Type, ChatType.CourseChat);
        _chats.Indexes.CreateOne(new CreateIndexModel<ChatDocument>(
            courseIdIndex,
            new CreateIndexOptions<ChatDocument>
            {
                Unique = true,
                PartialFilterExpression = courseChatFilter,
                Name = "courseId_unique_when_coursechat"
            }));

        var messageIndex = Builders<MessageDocument>.IndexKeys
            .Ascending(m => m.ChatId)
            .Descending(m => m.SentAt);
        _messages.Indexes.CreateOne(new CreateIndexModel<MessageDocument>(messageIndex,
            new CreateIndexOptions { Name = "chatId_sentAt_desc" }));

        var receiptIndex = Builders<ReadReceiptDocument>.IndexKeys
            .Ascending(r => r.ChatId)
            .Ascending(r => r.UserId);
        _receipts.Indexes.CreateOne(new CreateIndexModel<ReadReceiptDocument>(receiptIndex,
            new CreateIndexOptions { Unique = true, Name = "chatId_userId_unique" }));
    }

    public async Task<List<ChatDocument>> GetUserChatsAsync(string userId)
    {
        var filter = Builders<ChatDocument>.Filter.AnyEq(c => c.ParticipantIds, userId);
        return await _chats.Find(filter)
            .SortByDescending(c => c.LastMessageAt)
            .ToListAsync();
    }

    public async Task<ChatDocument?> GetChatByIdAsync(string chatId)
    {
        var filter = Builders<ChatDocument>.Filter.Eq(c => c.Id, chatId);
        return await _chats.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<ChatDocument?> GetByCourseIdAsync(string courseId)
    {
        var filter = Builders<ChatDocument>.Filter.And(
            Builders<ChatDocument>.Filter.Eq(c => c.Type, ChatType.CourseChat),
            Builders<ChatDocument>.Filter.Eq(c => c.CourseId, courseId)
        );
        return await _chats.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<ChatDocument> GetOrCreateDirectChatAsync(
        string userId1, string userName1,
        string userId2, string userName2)
    {
        var filter = Builders<ChatDocument>.Filter.And(
            Builders<ChatDocument>.Filter.Eq(c => c.Type, ChatType.DirectMessage),
            Builders<ChatDocument>.Filter.AnyEq(c => c.ParticipantIds, userId1),
            Builders<ChatDocument>.Filter.AnyEq(c => c.ParticipantIds, userId2)
        );

        var existing = await _chats.Find(filter).FirstOrDefaultAsync();
        if (existing != null)
            return existing;

        var chat = new ChatDocument
        {
            Type = ChatType.DirectMessage,
            ParticipantIds = new List<string> { userId1, userId2 },
            Participants = new List<ParticipantInfo>
            {
                new() { UserId = userId1, Name = userName1 },
                new() { UserId = userId2, Name = userName2 }
            },
            CreatedAt = DateTime.UtcNow
        };

        await _chats.InsertOneAsync(chat);
        return chat;
    }

    public async Task<ChatDocument> CreateCourseChatAsync(
        string courseId, string courseName,
        List<string> participantIds, List<string> participantNames,
        string? ownerId = null)
    {
        var participants = participantIds
            .Select((id, i) => new ParticipantInfo
            {
                UserId = id,
                Name = i < participantNames.Count ? participantNames[i] : id
            })
            .ToList();

        var chat = new ChatDocument
        {
            Type = ChatType.CourseChat,
            CourseId = courseId,
            CourseName = courseName,
            OwnerId = ownerId,
            ParticipantIds = participantIds,
            Participants = participants,
            CreatedAt = DateTime.UtcNow
        };

        await _chats.InsertOneAsync(chat);
        return chat;
    }

    public async Task<List<MessageDocument>> GetChatMessagesAsync(string chatId, int page, int pageSize)
    {
        var filter = Builders<MessageDocument>.Filter.Eq(m => m.ChatId, chatId);
        return await _messages.Find(filter)
            .SortByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<MessageDocument?> GetMessageByIdAsync(string messageId)
    {
        var filter = Builders<MessageDocument>.Filter.Eq(m => m.Id, messageId);
        return await _messages.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<MessageDocument> SendMessageAsync(MessageDocument message)
    {
        await _messages.InsertOneAsync(message);

        var chatFilter = Builders<ChatDocument>.Filter.Eq(c => c.Id, message.ChatId);
        var update = Builders<ChatDocument>.Update
            .Set(c => c.LastMessage, BuildMessagePreview(message))
            .Set(c => c.LastMessageAt, message.SentAt);
        await _chats.UpdateOneAsync(chatFilter, update);

        return message;
    }

    public async Task MarkMessagesAsReadAsync(string chatId, string userId)
    {
        var filter = Builders<ReadReceiptDocument>.Filter.And(
            Builders<ReadReceiptDocument>.Filter.Eq(r => r.ChatId, chatId),
            Builders<ReadReceiptDocument>.Filter.Eq(r => r.UserId, userId));

        var update = Builders<ReadReceiptDocument>.Update
            .Set(r => r.LastReadAt, DateTime.UtcNow)
            .SetOnInsert(r => r.ChatId, chatId)
            .SetOnInsert(r => r.UserId, userId);

        await _receipts.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var perChat = await GetUnreadCountsPerChatAsync(userId);
        return perChat.Values.Sum();
    }

    public async Task<Dictionary<string, int>> GetUnreadCountsPerChatAsync(string userId)
    {
        var chatFilter = Builders<ChatDocument>.Filter.AnyEq(c => c.ParticipantIds, userId);
        var chatIds = await _chats.Find(chatFilter)
            .Project(c => c.Id)
            .ToListAsync();

        if (chatIds.Count == 0) return new Dictionary<string, int>();

        var receiptFilter = Builders<ReadReceiptDocument>.Filter.And(
            Builders<ReadReceiptDocument>.Filter.In(r => r.ChatId, chatIds),
            Builders<ReadReceiptDocument>.Filter.Eq(r => r.UserId, userId));
        var receipts = await _receipts.Find(receiptFilter).ToListAsync();
        var lastReadByChat = receipts.ToDictionary(r => r.ChatId, r => r.LastReadAt);

        var result = new Dictionary<string, int>(chatIds.Count);
        foreach (var chatId in chatIds)
        {
            var lastReadAt = lastReadByChat.GetValueOrDefault(chatId, DateTime.MinValue);
            var msgFilter = Builders<MessageDocument>.Filter.And(
                Builders<MessageDocument>.Filter.Eq(m => m.ChatId, chatId),
                Builders<MessageDocument>.Filter.Ne(m => m.SenderId, userId),
                Builders<MessageDocument>.Filter.Gt(m => m.SentAt, lastReadAt));
            var count = (int)await _messages.CountDocumentsAsync(msgFilter);
            if (count > 0)
                result[chatId] = count;
        }

        return result;
    }

    public async Task<bool> DeleteMessageAsync(string messageId, string userId)
    {
        var filter = Builders<MessageDocument>.Filter.And(
            Builders<MessageDocument>.Filter.Eq(m => m.Id, messageId),
            Builders<MessageDocument>.Filter.Eq(m => m.SenderId, userId)
        );

        var deletedMessage = await _messages.FindOneAndDeleteAsync(filter);
        if (deletedMessage == null)
            return false;

        await RefreshChatLastMessageAsync(deletedMessage.ChatId);
        return true;
    }

    public async Task<bool> EditMessageAsync(string messageId, string userId, string newText, TimeSpan editWindow)
    {
        var threshold = DateTime.UtcNow - editWindow;
        var filter = Builders<MessageDocument>.Filter.And(
            Builders<MessageDocument>.Filter.Eq(m => m.Id, messageId),
            Builders<MessageDocument>.Filter.Eq(m => m.SenderId, userId),
            Builders<MessageDocument>.Filter.Gte(m => m.SentAt, threshold)
        );

        var update = Builders<MessageDocument>.Update
            .Set(m => m.Text, newText)
            .Set(m => m.IsEdited, true);

        var updatedMessage = await _messages.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<MessageDocument>
            {
                ReturnDocument = ReturnDocument.After
            });

        if (updatedMessage == null)
            return false;

        await RefreshChatLastMessageAsync(updatedMessage.ChatId);
        return true;
    }

    public async Task<bool> DeleteChatAsync(string chatId)
    {
        // Delete messages first to minimize orphan window
        await _messages.DeleteManyAsync(Builders<MessageDocument>.Filter.Eq(m => m.ChatId, chatId));
        var chatResult = await _chats.DeleteOneAsync(Builders<ChatDocument>.Filter.Eq(c => c.Id, chatId));
        return chatResult.DeletedCount > 0;
    }

    public async Task<bool> AddParticipantAsync(string chatId, string userId, string userName)
    {
        // Idempotent: only update if userId not already in ParticipantIds
        var filter = Builders<ChatDocument>.Filter.And(
            Builders<ChatDocument>.Filter.Eq(c => c.Id, chatId),
            Builders<ChatDocument>.Filter.Not(Builders<ChatDocument>.Filter.AnyEq(c => c.ParticipantIds, userId))
        );

        var update = Builders<ChatDocument>.Update
            .AddToSet(c => c.ParticipantIds, userId)
            .Push(c => c.Participants, new ParticipantInfo { UserId = userId, Name = userName });

        var result = await _chats.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> RemoveParticipantAsync(string chatId, string userId)
    {
        var filter = Builders<ChatDocument>.Filter.Eq(c => c.Id, chatId);
        var update = Builders<ChatDocument>.Update
            .Pull(c => c.ParticipantIds, userId)
            .PullFilter(c => c.Participants, p => p.UserId == userId);

        var result = await _chats.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task SetArchivedAsync(string chatId, bool archived)
    {
        var filter = Builders<ChatDocument>.Filter.Eq(c => c.Id, chatId);
        var update = Builders<ChatDocument>.Update.Set(c => c.IsArchived, archived);
        await _chats.UpdateOneAsync(filter, update);
    }

    private async Task RefreshChatLastMessageAsync(string chatId)
    {
        var latestMessage = await _messages.Find(Builders<MessageDocument>.Filter.Eq(m => m.ChatId, chatId))
            .SortByDescending(m => m.SentAt)
            .FirstOrDefaultAsync();

        var update = latestMessage == null
            ? Builders<ChatDocument>.Update
                .Set(c => c.LastMessage, null as string)
                .Set(c => c.LastMessageAt, null as DateTime?)
            : Builders<ChatDocument>.Update
                .Set(c => c.LastMessage, BuildMessagePreview(latestMessage))
                .Set(c => c.LastMessageAt, latestMessage.SentAt);

        await _chats.UpdateOneAsync(
            Builders<ChatDocument>.Filter.Eq(c => c.Id, chatId),
            update);
    }

    private static string BuildMessagePreview(MessageDocument message)
    {
        if (!string.IsNullOrWhiteSpace(message.Text))
            return message.Text;

        if (message.Attachments.Count == 1)
            return "[вложение]";
        if (message.Attachments.Count > 1)
            return $"[вложений: {message.Attachments.Count}]";

        return "Новое сообщение";
    }
}
