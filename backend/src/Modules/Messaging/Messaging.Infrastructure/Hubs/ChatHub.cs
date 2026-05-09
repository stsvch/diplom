using Messaging.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Messaging.Infrastructure.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;
    private readonly IChatConnectionTracker _connectionTracker;

    public ChatHub(
        IMessagingRepository repository,
        IChatBroadcaster broadcaster,
        IChatConnectionTracker connectionTracker)
    {
        _repository = repository;
        _broadcaster = broadcaster;
        _connectionTracker = connectionTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            _connectionTracker.AddConnection(userId, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId))
            _connectionTracker.RemoveConnection(userId, Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(string chatId)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Не авторизован.");

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null || !chat.ParticipantIds.Contains(userId))
            throw new HubException("Чат недоступен.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
    }

    public async Task MarkAsRead(string chatId)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Не авторизован.");

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            throw new HubException("Чат не найден.");
        if (!chat.ParticipantIds.Contains(userId))
            throw new HubException("Вы не участник чата.");

        await _repository.MarkMessagesAsReadAsync(chatId, userId);
        await _broadcaster.MessagesReadAsync(chatId, userId);
    }

    private string GetUserId()
    {
        return Context.User?.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }
}
