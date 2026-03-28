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

    public ChatHub(IMessagingRepository repository)
    {
        _repository = repository;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            // Join groups for each user's chat
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
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(string chatId)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return;

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null || !chat.ParticipantIds.Contains(userId))
            return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
    }

    public async Task SendMessage(string chatId, string text, List<AttachmentDto>? attachments = null)
    {
        var userId = GetUserId();
        var userName = GetUserName();

        if (string.IsNullOrEmpty(userId))
            return;

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null || !chat.ParticipantIds.Contains(userId))
            return;

        var messageDoc = new MessageDocument
        {
            ChatId = chatId,
            SenderId = userId,
            SenderName = userName,
            Text = text,
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

        var messageDto = new MessageDto
        {
            Id = saved.Id,
            ChatId = saved.ChatId,
            SenderId = saved.SenderId,
            SenderName = saved.SenderName,
            Text = saved.Text,
            Attachments = saved.Attachments.Select(a => new AttachmentDto
            {
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList(),
            SentAt = saved.SentAt,
            ReadBy = saved.ReadBy,
            IsEdited = saved.IsEdited
        };

        await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", messageDto);
    }

    public async Task MarkAsRead(string chatId)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return;

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null || !chat.ParticipantIds.Contains(userId))
            return;

        await _repository.MarkMessagesAsReadAsync(chatId, userId);

        // Notify other participants
        await Clients.OthersInGroup($"chat_{chatId}").SendAsync("MessagesRead", chatId, userId);
    }

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
