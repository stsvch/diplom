using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifications.Application.Notifications.Commands.CreateNotification;
using Notifications.Application.Notifications.Commands.DeleteNotification;
using Notifications.Application.Notifications.Commands.MarkAllAsRead;
using Notifications.Application.Notifications.Commands.MarkAsRead;
using Notifications.Application.Notifications.Queries.GetUnreadCount;
using Notifications.Application.Notifications.Queries.GetUserNotifications;
using Notifications.Domain.Enums;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] NotificationType? type,
        [FromQuery] bool? isRead,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetUserNotificationsQuery(userId, type, isRead, page, pageSize), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "NOTIFICATIONS_FETCH_FAILED"));
        return Ok(result.Value);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetUnreadCountQuery(userId), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "UNREAD_COUNT_FAILED"));
        return Ok(new { count = result.Value });
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new MarkAsReadCommand(id, userId), ct);
        if (result.IsFailure) return NotFound(ApiError.FromMessage(result.Error!, "NOTIFICATION_NOT_FOUND"));
        return Ok(new { message = "Уведомление отмечено как прочитанное." });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new MarkAllAsReadCommand(userId), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "MARK_ALL_READ_FAILED"));
        return Ok(new { message = "Все уведомления отмечены как прочитанные." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new DeleteNotificationCommand(id, userId), ct);
        if (result.IsFailure) return NotFound(ApiError.FromMessage(result.Error!, "NOTIFICATION_NOT_FOUND"));
        return Ok(new { message = "Уведомление удалено." });
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateNotificationCommand(request.UserId, request.Type, request.Title, request.Message, request.LinkUrl), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "NOTIFICATION_CREATE_FAILED"));
        return Ok(result.Value);
    }
}

public record CreateNotificationRequest(string UserId, NotificationType Type, string Title, string Message, string? LinkUrl);
