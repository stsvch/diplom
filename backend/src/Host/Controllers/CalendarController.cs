using Calendar.Application.Calendar.Commands.CreateCalendarEvent;
using Calendar.Application.Calendar.Commands.DeleteCalendarEvent;
using Calendar.Application.Calendar.Queries.GetMonthEvents;
using Calendar.Application.Calendar.Queries.GetUpcomingEvents;
using Calendar.Domain.Enums;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/calendar")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IMediator _mediator;
    public CalendarController(IMediator mediator) => _mediator = mediator;

    [HttpGet("events")]
    public async Task<IActionResult> GetMonthEvents([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetMonthEventsQuery(userId, year, month), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "CALENDAR_FETCH_FAILED"));
        return Ok(result.Value);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int count = 10, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetUpcomingEventsQuery(userId, count), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "CALENDAR_FETCH_FAILED"));
        return Ok(result.Value);
    }

    [HttpPost("events")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create([FromBody] CreateCalendarEventRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(
            new CreateCalendarEventCommand(userId, request.CourseId, request.Title, request.Description,
                request.EventDate, request.EventTime, request.Type, request.SourceType, request.SourceId), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "CALENDAR_CREATE_FAILED"));
        return Ok(result.Value);
    }

    [HttpDelete("events/{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new DeleteCalendarEventCommand(id, userId), ct);
        if (result.IsFailure) return NotFound(ApiError.FromMessage(result.Error!, "CALENDAR_EVENT_NOT_FOUND"));
        return Ok(new { message = "Событие удалено." });
    }
}

public record CreateCalendarEventRequest(
    Guid? CourseId,
    string Title,
    string? Description,
    DateTime EventDate,
    string? EventTime,
    CalendarEventType Type,
    string? SourceType,
    Guid? SourceId);
