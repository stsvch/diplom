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
    private readonly IChatBroadcaster _broadcaster;

    public ChatsController(IMessagingRepository repository, IChatBroadcaster broadcaster)
    {
        _repository = repository;
        _broadcaster = broadcaster;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserChats()
    {
        var userId = GetUserId();
        var chats = await _repository.GetUserChatsAsync(userId);
        var unreadPerChat = await _repository.GetUnreadCountsPerChatAsync(userId);
        var dtos = chats.Select(c => MapChatToDto(c, unreadPerChat.GetValueOrDefault(c.Id, 0))).ToList();
        return Ok(dtos);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _repository.GetUnreadCountAsync(userId);
        return Ok(new { count });
    }

    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetChatById(string chatId)
    {
        var userId = GetUserId();
        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(ApiError.FromMessage("Чат не найден", "CHAT_NOT_FOUND"));
        if (!chat.ParticipantIds.Contains(userId))
            return Forbid();

        var unread = await _repository.GetUnreadCountsPerChatAsync(userId);
        return Ok(MapChatToDto(chat, unread.GetValueOrDefault(chatId, 0)));
    }

    [HttpPost("direct")]
    public async Task<IActionResult> GetOrCreateDirectChat([FromBody] CreateDirectChatRequest request)
    {
        var userId = GetUserId();
        var userName = GetUserName();
        if (userId == request.RecipientId)
            return BadRequest(ApiError.FromMessage("Нельзя создать чат с самим собой", "INVALID_RECIPIENT"));

        var chat = await _repository.GetOrCreateDirectChatAsync(
            userId, userName,
            request.RecipientId, request.RecipientName);

        await _broadcaster.InstructUserJoinChatAsync(userId, chat.Id);
        await _broadcaster.InstructUserJoinChatAsync(request.RecipientId, chat.Id);

        return Ok(MapChatToDto(chat, 0));
    }

    [HttpPost("course")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateCourseChat([FromBody] CreateCourseChatRequest request)
    {
        var userId = GetUserId();
        var chat = await _repository.CreateCourseChatAsync(
            request.CourseId, request.CourseName,
            request.ParticipantIds, request.ParticipantNames ?? new List<string>(),
            ownerId: userId);

        return Ok(MapChatToDto(chat, 0));
    }

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
        if (chat.IsArchived)
            return BadRequest(ApiError.FromMessage("Чат архивирован", "CHAT_ARCHIVED"));

        var hasText = !string.IsNullOrWhiteSpace(request.Text);
        var hasAttachments = request.Attachments is { Count: > 0 };
        if (!hasText && !hasAttachments)
            return BadRequest(ApiError.FromMessage("Пустое сообщение", "EMPTY_MESSAGE"));

        var message = new MessageDocument
        {
            ChatId = chatId,
            SenderId = userId,
            SenderName = userName,
            Text = request.Text ?? string.Empty,
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
        var dto = MapMessageToDto(saved);
        await _broadcaster.MessageSentAsync(chatId, dto);
        return Ok(dto);
    }

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
        await _broadcaster.MessagesReadAsync(chatId, userId);
        return Ok(new { message = "Сообщения отмечены как прочитанные" });
    }

    [HttpPost("{chatId}/hide")]
    public async Task<IActionResult> HideChat(string chatId)
    {
        var userId = GetUserId();
        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(ApiError.FromMessage("Чат не найден", "CHAT_NOT_FOUND"));
        if (!chat.ParticipantIds.Contains(userId))
            return Forbid();

        await _repository.HideChatAsync(chatId, userId);
        return Ok(new { message = "Чат скрыт" });
    }

    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteChat(string chatId)
    {
        var userId = GetUserId();
        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(ApiError.FromMessage("Чат не найден", "CHAT_NOT_FOUND"));

        if (chat.Type != "CourseChat")
            return BadRequest(ApiError.FromMessage(
                "Прямой чат нельзя удалить для всех. Используйте 'скрыть у себя'.",
                "DIRECT_CHAT_CANNOT_BE_DELETED"));

        if (chat.OwnerId != userId)
            return Forbid();

        await _repository.DeleteChatAsync(chatId);
        await _broadcaster.ChatDeletedAsync(chatId);
        return Ok(new { message = "Чат удалён" });
    }

    [HttpPost("{chatId}/participants")]
    public async Task<IActionResult> AddParticipant(string chatId, [FromBody] AddParticipantRequest request)
    {
        var userId = GetUserId();
        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(ApiError.FromMessage("Чат не найден", "CHAT_NOT_FOUND"));
        if (chat.Type != "CourseChat")
            return BadRequest(ApiError.FromMessage("Только для курсового чата", "NOT_COURSE_CHAT"));
        if (chat.OwnerId != userId)
            return Forbid();

        var added = await _repository.AddParticipantAsync(chatId, request.UserId, request.UserName);
        if (!added)
            return Ok(new { message = "Участник уже в чате" });

        await _broadcaster.ParticipantAddedAsync(chatId, new ParticipantDto { UserId = request.UserId, Name = request.UserName });
        await _broadcaster.InstructUserJoinChatAsync(request.UserId, chatId);
        return Ok(new { message = "Участник добавлен" });
    }

    [HttpDelete("{chatId}/participants/{participantId}")]
    public async Task<IActionResult> RemoveParticipant(string chatId, string participantId)
    {
        var userId = GetUserId();
        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound(ApiError.FromMessage("Чат не найден", "CHAT_NOT_FOUND"));
        if (chat.Type != "CourseChat")
            return BadRequest(ApiError.FromMessage("Только для курсового чата", "NOT_COURSE_CHAT"));
        if (chat.OwnerId != userId)
            return Forbid();

        var removed = await _repository.RemoveParticipantAsync(chatId, participantId);
        if (!removed)
            return NotFound(ApiError.FromMessage("Участник не найден", "PARTICIPANT_NOT_FOUND"));

        await _broadcaster.RemoveUserFromChatAsync(participantId, chatId);
        await _broadcaster.ParticipantRemovedAsync(chatId, participantId);
        return Ok(new { message = "Участник удалён" });
    }

    private string GetUserId() =>
        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    private string GetUserName()
    {
        var given = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "";
        var surname = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";
        var name = $"{given} {surname}".Trim();
        if (string.IsNullOrEmpty(name))
            name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Unknown";
        return name;
    }

    private static ChatDto MapChatToDto(ChatDocument chat, int unreadCount) => new()
    {
        Id = chat.Id,
        Type = chat.Type,
        CourseId = chat.CourseId,
        CourseName = chat.CourseName,
        OwnerId = chat.OwnerId,
        IsArchived = chat.IsArchived,
        Participants = chat.Participants.Select(p => new ParticipantDto
        {
            UserId = p.UserId,
            Name = p.Name
        }).ToList(),
        LastMessage = chat.LastMessage,
        LastMessageAt = chat.LastMessageAt,
        UnreadCount = unreadCount
    };

    internal static MessageDto MapMessageToDto(MessageDocument msg) => new()
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

public class AddParticipantRequest
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}
