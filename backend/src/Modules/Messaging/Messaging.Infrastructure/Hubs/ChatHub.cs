using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain.Enums;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Domain.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Messaging.Infrastructure.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessagingRepository _repository;
    private readonly INotificationDispatcher _notifications;
    private readonly IChatBroadcaster _broadcaster;
    private readonly IChatConnectionTracker _connectionTracker;

    public ChatHub(
        IMessagingRepository repository,
        INotificationDispatcher notifications,
        IChatBroadcaster broadcaster,
        IChatConnectionTracker connectionTracker)
    {
        _repository = repository;
        _notifications = notifications;
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

            var chats = await _repository.GetUserChatsAsync(userId);
            foreach (var chat in chats)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chat.Id}");
            }
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

    public async Task SendMessage(string chatId, string text, List<AttachmentDto>? attachments = null)
    {
        var userId = GetUserId();
        var userName = GetUserName();

        if (string.IsNullOrEmpty(userId))
            throw new HubException("Не авторизован.");

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            throw new HubException("Чат не найден.");
        if (!chat.ParticipantIds.Contains(userId))
            throw new HubException("Вы не участник чата.");
        if (chat.IsArchived)
            throw new HubException("Чат архивирован.");

        var hasText = !string.IsNullOrWhiteSpace(text);
        var hasAttachments = attachments is { Count: > 0 };
        if (!hasText && !hasAttachments)
            throw new HubException("Пустое сообщение.");

        var messageDoc = new MessageDocument
        {
            ChatId = chatId,
            SenderId = userId,
            SenderName = userName,
            Text = text ?? string.Empty,
            Attachments = attachments?.Select(a => new MessageAttachment
            {
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList() ?? new List<MessageAttachment>(),
            SentAt = DateTime.UtcNow,
            ReadBy = new List<string> { userId }
        };

        var saved = await _repository.SendMessageAsync(messageDoc);
        var messageDto = MapToDto(saved);

        await _broadcaster.MessageSentAsync(chatId, messageDto);

        var recipients = chat.ParticipantIds.Where(id => id != userId).ToList();
        if (recipients.Count > 0)
        {
            var safeText = text ?? string.Empty;
            var preview = safeText.Length > 80 ? safeText.Substring(0, 80) + "…" : safeText;
            if (string.IsNullOrEmpty(preview) && messageDoc.Attachments.Count > 0)
                preview = $"[вложение: {messageDoc.Attachments.Count}]";
            var notifications = recipients.Select(rid => new NotificationRequest(
                rid, NotificationType.Message, "Новое сообщение",
                $"{userName}: {preview}", $"/messages/{chatId}")).ToList();
            await _notifications.PublishManyAsync(notifications);
        }
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

    private static MessageDto MapToDto(MessageDocument msg) => new()
    {
        Id = msg.Id,
        ChatId = msg.ChatId,
        SenderId = msg.SenderId,
        SenderName = msg.SenderName,
        Text = msg.Text,
        Attachments = msg.Attachments.Select(a => new AttachmentDto
        {
            FileName = a.FileName,
            FileUrl = a.FileUrl,
            ContentType = a.ContentType,
            FileSize = a.FileSize
        }).ToList(),
        SentAt = msg.SentAt,
        ReadBy = msg.ReadBy,
        IsEdited = msg.IsEdited
    };

    private string GetUserId()
    {
        return Context.User?.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    private string GetUserName()
    {
        var given = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "";
        var surname = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";
        var name = $"{given} {surname}".Trim();
        if (string.IsNullOrEmpty(name))
            name = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Unknown";
        return name;
    }
}
