// Файл: ChatsController.cs
using EduPlatform.Shared.Application.Models;
using MediatR;
using Messaging.Application.Commands.AddParticipant;
using Messaging.Application.Commands.CreateCourseChat;
using Messaging.Application.Commands.CreateDirectChat;
using Messaging.Application.Commands.DeleteChat;
using Messaging.Application.Commands.MarkAsRead;
using Messaging.Application.Commands.RemoveParticipant;
using Messaging.Application.Commands.SendMessage;
using Messaging.Application.DTOs;
using Messaging.Application.Queries.GetChatById;
using Messaging.Application.Queries.GetChatMessages;
using Messaging.Application.Queries.GetUnreadCount;
using Messaging.Application.Queries.GetUserChats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

// Контроллер ChatsController группирует HTTP-эндпоинты и делегирует работу в прикладные сценарии.
[ApiController]
[Route("api/chats")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserChats(CancellationToken ct)
    {
        var dtos = await _mediator.Send(new GetUserChatsQuery(GetUserId()), ct);
        return Ok(dtos);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var count = await _mediator.Send(new GetUnreadCountQuery(GetUserId()), ct);
        return Ok(new { count });
    }

    [HttpGet("{chatId}")]
    [Authorize(Policy = "ChatParticipant")]
    public async Task<IActionResult> GetChatById(string chatId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChatByIdQuery(chatId, GetUserId()), ct);
        return result.IsFailure
            ? NotFound(ApiError.FromMessage(result.Error!, "CHAT_NOT_FOUND"))
            : Ok(result.Value);
    }

    [HttpPost("direct")]
    public async Task<IActionResult> GetOrCreateDirectChat([FromBody] CreateDirectChatRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateDirectChatCommand(
            GetUserId(), GetUserName(), request.RecipientId, request.RecipientName), ct);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "INVALID_RECIPIENT"))
            : Ok(result.Value);
    }

    [HttpPost("course")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateCourseChat([FromBody] CreateCourseChatRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCourseChatCommand(
            GetUserId(), request.CourseId, request.CourseName,
            request.ParticipantIds, request.ParticipantNames), ct);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "COURSE_CHAT_CREATE_FAILED"))
            : Ok(result.Value);
    }

    [HttpGet("{chatId}/messages")]
    [Authorize(Policy = "ChatParticipant")]
    public async Task<IActionResult> GetChatMessages(
        string chatId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var dtos = await _mediator.Send(new GetChatMessagesQuery(chatId, page, pageSize), ct);
        return Ok(dtos);
    }

    [HttpPost("{chatId}/messages")]
    [Authorize(Policy = "ChatParticipant")]
    public async Task<IActionResult> SendMessage(string chatId, [FromBody] SendMessageDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendMessageCommand(
            chatId, GetUserId(), GetUserName(), request.Text ?? string.Empty, request.Attachments), ct);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "MESSAGE_SEND_FAILED"))
            : Ok(result.Value);
    }

    [HttpPut("{chatId}/read")]
    [Authorize(Policy = "ChatParticipant")]
    public async Task<IActionResult> MarkAsRead(string chatId, CancellationToken ct)
    {
        await _mediator.Send(new MarkAsReadCommand(chatId, GetUserId()), ct);
        return Ok(new { message = "Сообщения отмечены как прочитанные" });
    }

    [HttpDelete("{chatId}")]
    [Authorize(Policy = "CourseChatOwner")]
    public async Task<IActionResult> DeleteChat(string chatId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteChatCommand(chatId), ct);
        return Ok(new { message = "Чат удалён" });
    }

    [HttpPost("{chatId}/participants")]
    [Authorize(Policy = "CourseChatOwner")]
    public async Task<IActionResult> AddParticipant(string chatId, [FromBody] AddParticipantRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddParticipantCommand(chatId, request.UserId, request.UserName), ct);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "ADD_PARTICIPANT_FAILED"));

        return Ok(new { message = result.Value ? "Участник добавлен" : "Участник уже в чате" });
    }

    [HttpDelete("{chatId}/participants/{participantId}")]
    [Authorize(Policy = "CourseChatOwner")]
    public async Task<IActionResult> RemoveParticipant(string chatId, string participantId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveParticipantCommand(chatId, participantId), ct);
        return result.IsFailure
            ? NotFound(ApiError.FromMessage(result.Error!, "PARTICIPANT_NOT_FOUND"))
            : Ok(new { message = "Участник удалён" });
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
}

// Класс CreateDirectChatRequest инкапсулирует ответственность соответствующего сценария.
public class CreateDirectChatRequest
{
    public string RecipientId { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
}

// Класс CreateCourseChatRequest инкапсулирует ответственность соответствующего сценария.
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
