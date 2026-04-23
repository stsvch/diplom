using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Infrastructure.Services;

public class SignalRChatBroadcaster : IChatBroadcaster
{
    private readonly IHubContext<ChatHub> _hub;
    private readonly IChatConnectionTracker _connectionTracker;

    public SignalRChatBroadcaster(IHubContext<ChatHub> hub, IChatConnectionTracker connectionTracker)
    {
        _hub = hub;
        _connectionTracker = connectionTracker;
    }

    public Task MessageSentAsync(string chatId, MessageDto message) =>
        _hub.Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", message);

    public Task MessageEditedAsync(string chatId, MessageDto message) =>
        _hub.Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessageEdited", message);

    public Task MessageDeletedAsync(string chatId, string messageId) =>
        _hub.Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessageDeleted", chatId, messageId);

    public Task MessagesReadAsync(string chatId, string userId) =>
        _hub.Clients.Group($"chat_{chatId}").SendAsync("MessagesRead", chatId, userId);

    public Task ParticipantAddedAsync(string chatId, ParticipantDto participant) =>
        _hub.Clients.Group($"chat_{chatId}").SendAsync("ParticipantAdded", chatId, participant);

    public Task ParticipantRemovedAsync(string chatId, string userId) =>
        _hub.Clients.Group($"chat_{chatId}").SendAsync("ParticipantRemoved", chatId, userId);

    public Task ChatArchivedAsync(string chatId) =>
        _hub.Clients.Group($"chat_{chatId}").SendAsync("ChatArchived", chatId);

    public Task ChatDeletedAsync(string chatId) =>
        _hub.Clients.Group($"chat_{chatId}").SendAsync("ChatDeleted", chatId);

    public Task InstructUserJoinChatAsync(string userId, string chatId) =>
        _hub.Clients.Group($"user_{userId}").SendAsync("JoinChatInstruction", chatId);

    public async Task RemoveUserFromChatAsync(string userId, string chatId)
    {
        var chatGroup = $"chat_{chatId}";
        var tasks = _connectionTracker
            .GetConnections(userId)
            .Select(connectionId => _hub.Groups.RemoveFromGroupAsync(connectionId, chatGroup));

        await Task.WhenAll(tasks);
        await _hub.Clients.Group($"user_{userId}").SendAsync("RemovedFromChat", chatId);
    }
}
