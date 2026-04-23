using Messaging.Application.DTOs;

namespace Messaging.Application.Interfaces;

public interface IChatBroadcaster
{
    Task MessageSentAsync(string chatId, MessageDto message);
    Task MessageEditedAsync(string chatId, MessageDto message);
    Task MessageDeletedAsync(string chatId, string messageId);
    Task MessagesReadAsync(string chatId, string userId);

    Task ParticipantAddedAsync(string chatId, ParticipantDto participant);
    Task ParticipantRemovedAsync(string chatId, string userId);

    Task ChatArchivedAsync(string chatId);
    Task ChatDeletedAsync(string chatId);

    Task InstructUserJoinChatAsync(string userId, string chatId);
    Task RemoveUserFromChatAsync(string userId, string chatId);
}
