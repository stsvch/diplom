using Messaging.Domain.Documents;

namespace Messaging.Application.Interfaces;

public interface IMessagingRepository
{
    Task<List<ChatDocument>> GetUserChatsAsync(string userId);
    Task<ChatDocument?> GetChatByIdAsync(string chatId);
    Task<ChatDocument> GetOrCreateDirectChatAsync(string userId1, string userName1, string userId2, string userName2);
    Task<ChatDocument> CreateCourseChatAsync(string courseId, string courseName, List<string> participantIds, List<string> participantNames);
    Task<List<MessageDocument>> GetChatMessagesAsync(string chatId, int page, int pageSize);
    Task<MessageDocument> SendMessageAsync(MessageDocument message);
    Task MarkMessagesAsReadAsync(string chatId, string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<bool> DeleteMessageAsync(string messageId, string userId);
}
