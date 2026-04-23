using Messaging.Domain.Documents;

namespace Messaging.Application.Interfaces;

public interface IMessagingRepository
{
    Task<List<ChatDocument>> GetUserChatsAsync(string userId);
    Task<ChatDocument?> GetChatByIdAsync(string chatId);
    Task<ChatDocument?> GetByCourseIdAsync(string courseId);
    Task<ChatDocument> GetOrCreateDirectChatAsync(string userId1, string userName1, string userId2, string userName2);
    Task<ChatDocument> CreateCourseChatAsync(string courseId, string courseName, List<string> participantIds, List<string> participantNames, string? ownerId = null);
    Task<List<MessageDocument>> GetChatMessagesAsync(string chatId, int page, int pageSize);
    Task<MessageDocument?> GetMessageByIdAsync(string messageId);
    Task<MessageDocument> SendMessageAsync(MessageDocument message);
    Task MarkMessagesAsReadAsync(string chatId, string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<Dictionary<string, int>> GetUnreadCountsPerChatAsync(string userId);
    Task<bool> DeleteMessageAsync(string messageId, string userId);
    Task<bool> EditMessageAsync(string messageId, string userId, string newText, TimeSpan editWindow);
    Task<bool> DeleteChatAsync(string chatId);
    Task<bool> AddParticipantAsync(string chatId, string userId, string userName);
    Task<bool> RemoveParticipantAsync(string chatId, string userId);
    Task SetArchivedAsync(string chatId, bool archived);
    Task HideChatAsync(string chatId, string userId);
}
