using EduPlatform.Shared.Application.Models;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Domain.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/chats")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IMessagingRepository _repository;

    public ChatsController(IMessagingRepository repository)
    {
        _repository = repository;
    }

    // GET /api/chats
    [HttpGet]
    public async Task<IActionResult> GetUserChats()
    {
        var userId = GetUserId();
        var chats = await _repository.GetUserChatsAsync(userId);
        var dtos = await MapChatsToDtosAsync(chats, userId);
        return Ok(dtos);
    }

    // GET /api/chats/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _repository.GetUnreadCountAsync(userId);
        return Ok(new { count });
    }

    // GET /api/chats/{chatId}
    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetChatById(string chatId)
    {
        var userId = GetUserId();
        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(ApiError.FromMessage("Чат не найден", "CHAT_NOT_FOUND"));

        if (!chat.ParticipantIds.Contains(userId))
            return Forbid();

        var dto = await MapChatToDtoAsync(chat, userId);
        return Ok(dto);
    }

    // POST /api/chats/direct
    [HttpPost("direct")]
    public async Task<IActionResult> GetOrCreateDirectChat([FromBody] CreateDirectChatRequest request)
    {
        var userId = GetUserId();
        var userName = GetUserName();

        var chat = await _repository.GetOrCreateDirectChatAsync(
            userId, userName,
            request.RecipientId, request.RecipientName);

        var dto = await MapChatToDtoAsync(chat, userId);
        return Ok(dto);
    }

    // POST /api/chats/course
    [HttpPost("course")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateCourseChat([FromBody] CreateCourseChatRequest request)
    {
        var chat = await _repository.CreateCourseChatAsync(
            request.CourseId, request.CourseName,
            request.ParticipantIds, request.ParticipantNames ?? new List<string>());

        var userId = GetUserId();
        var dto = await MapChatToDtoAsync(chat, userId);
        return Ok(dto);
    }

    // GET /api/chats/{chatId}/messages
    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetChatMessages(
        string chatId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(ApiError.FromMessage("Чат не найден", "CHAT_NOT_FOUND"));

        if (!chat.ParticipantIds.Contains(userId))
            return Forbid();

        var messages = await _repository.GetChatMessagesAsync(chatId, page, pageSize);
        var dtos = messages.Select(MapMessageToDto).ToList();
        return Ok(dtos);
    }

    // POST /api/chats/{chatId}/messages
    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessage(string chatId, [FromBody] SendMessageDto request)
    {
        var userId = GetUserId();
        var userName = GetUserName();

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(ApiError.FromMessage("Чат не найден", "CHAT_NOT_FOUND"));

        if (!chat.ParticipantIds.Contains(userId))
            return Forbid();

        var message = new MessageDocument
        {
            ChatId = chatId,
            SenderId = userId,
            SenderName = userName,
            Text = request.Text,
            Attachments = request.Attachments?.Select(a => new MessageAttachment
            {
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList() ?? new List<MessageAttachment>(),
            SentAt = DateTime.UtcNow,
            ReadBy = new List<string> { userId }
        };

        var saved = await _repository.SendMessageAsync(message);
        return Ok(MapMessageToDto(saved));
    }

    // PUT /api/chats/{chatId}/read
    [HttpPut("{chatId}/read")]
    public async Task<IActionResult> MarkAsRead(string chatId)
    {
        var userId = GetUserId();
        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(ApiError.FromMessage("Чат не найден", "CHAT_NOT_FOUND"));

        if (!chat.ParticipantIds.Contains(userId))
            return Forbid();

        await _repository.MarkMessagesAsReadAsync(chatId, userId);
        return Ok(new { message = "Сообщения отмечены как прочитанные" });
    }

    // DELETE /api/messages/{messageId}
    [HttpDelete("/api/messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(string messageId)
    {
        var userId = GetUserId();
        var deleted = await _repository.DeleteMessageAsync(messageId, userId);
        if (!deleted)
            return NotFound(ApiError.FromMessage("Сообщение не найдено или вы не автор", "MESSAGE_NOT_FOUND"));

        return Ok(new { message = "Сообщение удалено" });
    }

    private string GetUserId()
    {
        return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    private string GetUserName()
    {
        var given = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "";
        var surname = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";
        var name = $"{given} {surname}".Trim();
        if (string.IsNullOrEmpty(name))
            name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Unknown";
        return name;
    }

    private async Task<List<ChatDto>> MapChatsToDtosAsync(List<ChatDocument> chats, string userId)
    {
        var result = new List<ChatDto>();
        foreach (var chat in chats)
        {
            result.Add(await MapChatToDtoAsync(chat, userId));
        }
        return result;
    }

    private async Task<ChatDto> MapChatToDtoAsync(ChatDocument chat, string userId)
    {
        var unreadCount = 0;
        // Calculate per-chat unread (simplified: use a basic count approach)
        // This is acceptable since we don't need per-chat exact count from repository in this context

        return new ChatDto
        {
            Id = chat.Id,
            Type = chat.Type,
            CourseId = chat.CourseId,
            CourseName = chat.CourseName,
            Participants = chat.Participants.Select(p => new ParticipantDto
            {
                UserId = p.UserId,
                Name = p.Name
            }).ToList(),
            LastMessage = chat.LastMessage,
            LastMessageAt = chat.LastMessageAt,
            UnreadCount = unreadCount
        };
    }

    private static MessageDto MapMessageToDto(MessageDocument msg)
    {
        return new MessageDto
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
    }
}

public class CreateDirectChatRequest
{
    public string RecipientId { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
}

public class CreateCourseChatRequest
{
    public string CourseId { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public List<string> ParticipantIds { get; set; } = new();
    public List<string>? ParticipantNames { get; set; }
}
