using Calendar.Application.Calendar.Commands.CreateCalendarEvent;
using Calendar.Application.Calendar.Commands.DeleteCalendarEvent;
using Calendar.Application.Calendar.Queries.GetMonthEvents;
using Calendar.Application.Calendar.Queries.GetUpcomingEvents;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain.Enums;
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
        if (year < 2000 || year > 2100)
            return BadRequest(ApiError.FromMessage("Год должен быть в диапазоне 2000-2100.", "CALENDAR_INVALID_YEAR"));
        if (month < 1 || month > 12)
            return BadRequest(ApiError.FromMessage("Месяц должен быть от 1 до 12.", "CALENDAR_INVALID_MONTH"));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetMonthEventsQuery(userId, year, month), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "CALENDAR_FETCH_FAILED"));
        return Ok(result.Value);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int count = 10, CancellationToken ct = default)
    {
        if (count < 1 || count > 50)
            return BadRequest(ApiError.FromMessage("Количество событий должно быть от 1 до 50.", "CALENDAR_INVALID_COUNT"));

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
        if (result.IsFailure)
        {
            if (result.Error == "Событие не найдено.")
                return NotFound(ApiError.FromMessage(result.Error!, "CALENDAR_EVENT_NOT_FOUND"));
            if (result.Error == "Нет прав на удаление этого события.")
                return Forbid();

            return BadRequest(ApiError.FromMessage(result.Error!, "CALENDAR_DELETE_FAILED"));
        }

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
