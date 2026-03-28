using Messaging.Application.Interfaces;
using Messaging.Domain.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Messaging.Infrastructure.Repositories;

public class MongoMessagingRepository : IMessagingRepository
{
    private readonly IMongoCollection<ChatDocument> _chats;
    private readonly IMongoCollection<MessageDocument> _messages;

    public MongoMessagingRepository(IMongoDatabase database)
    {
        _chats = database.GetCollection<ChatDocument>("chats");
        _messages = database.GetCollection<MessageDocument>("messages");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Index on ParticipantIds for chats
        var chatIndex = Builders<ChatDocument>.IndexKeys.Ascending(c => c.ParticipantIds);
        _chats.Indexes.CreateOne(new CreateIndexModel<ChatDocument>(chatIndex));

        // Index on ChatId for messages
        var messageIndex = Builders<MessageDocument>.IndexKeys.Ascending(m => m.ChatId);
        _messages.Indexes.CreateOne(new CreateIndexModel<MessageDocument>(messageIndex));
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

    public async Task<ChatDocument> GetOrCreateDirectChatAsync(
        string userId1, string userName1,
        string userId2, string userName2)
    {
        // Find existing direct chat with both participants
        var filter = Builders<ChatDocument>.Filter.And(
            Builders<ChatDocument>.Filter.Eq(c => c.Type, "DirectMessage"),
            Builders<ChatDocument>.Filter.AnyEq(c => c.ParticipantIds, userId1),
            Builders<ChatDocument>.Filter.AnyEq(c => c.ParticipantIds, userId2)
        );

        var existing = await _chats.Find(filter).FirstOrDefaultAsync();
        if (existing != null)
            return existing;

        var chat = new ChatDocument
        {
            Type = "DirectMessage",
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
        List<string> participantIds, List<string> participantNames)
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
            Type = "CourseChat",
            CourseId = courseId,
            CourseName = courseName,
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

    public async Task<MessageDocument> SendMessageAsync(MessageDocument message)
    {
        await _messages.InsertOneAsync(message);

        // Update chat's LastMessage and LastMessageAt
        var update = Builders<ChatDocument>.Update
            .Set(c => c.LastMessage, message.Text)
            .Set(c => c.LastMessageAt, message.SentAt);

        var filter = Builders<ChatDocument>.Filter.Eq(c => c.Id, message.ChatId);
        await _chats.UpdateOneAsync(filter, update);

        return message;
    }

    public async Task MarkMessagesAsReadAsync(string chatId, string userId)
    {
        // Update all messages in chat where ReadBy doesn't contain userId
        var filter = Builders<MessageDocument>.Filter.And(
            Builders<MessageDocument>.Filter.Eq(m => m.ChatId, chatId),
            Builders<MessageDocument>.Filter.Not(
                Builders<MessageDocument>.Filter.AnyEq(m => m.ReadBy, userId)
            )
        );

        var update = Builders<MessageDocument>.Update.AddToSet(m => m.ReadBy, userId);
        await _messages.UpdateManyAsync(filter, update);
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        // Get all chats where user is participant
        var chatFilter = Builders<ChatDocument>.Filter.AnyEq(c => c.ParticipantIds, userId);
        var chatIds = await _chats.Find(chatFilter)
            .Project(c => c.Id)
            .ToListAsync();

        if (!chatIds.Any())
            return 0;

        // Count messages where sender != userId and ReadBy doesn't contain userId
        var messageFilter = Builders<MessageDocument>.Filter.And(
            Builders<MessageDocument>.Filter.In(m => m.ChatId, chatIds),
            Builders<MessageDocument>.Filter.Ne(m => m.SenderId, userId),
            Builders<MessageDocument>.Filter.Not(
                Builders<MessageDocument>.Filter.AnyEq(m => m.ReadBy, userId)
            )
        );

        return (int)await _messages.CountDocumentsAsync(messageFilter);
    }

    public async Task<bool> DeleteMessageAsync(string messageId, string userId)
    {
        var filter = Builders<MessageDocument>.Filter.And(
            Builders<MessageDocument>.Filter.Eq(m => m.Id, messageId),
            Builders<MessageDocument>.Filter.Eq(m => m.SenderId, userId)
        );

        var result = await _messages.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }
}
