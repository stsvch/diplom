// Файл: ScheduleController.cs
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduling.Application.Scheduling.Commands.BookSlot;
using Scheduling.Application.Scheduling.Commands.CancelBooking;
using Scheduling.Application.Scheduling.Commands.CancelSlot;
using Scheduling.Application.Scheduling.Commands.CompleteSlot;
using Scheduling.Application.Scheduling.Commands.CreateAvailability;
using Scheduling.Application.Scheduling.Commands.DeleteAvailability;
using Scheduling.Application.Scheduling.Commands.UpdateSlot;
using Scheduling.Application.Scheduling.Queries.GetMyAvailability;
using Scheduling.Application.Scheduling.Queries.GetMyBookings;
using Scheduling.Application.Scheduling.Queries.GetSlotById;
using Scheduling.Application.Scheduling.Queries.GetTeacherCalendar;
using Scheduling.Application.Scheduling.Queries.GetTeacherSlots;
using Scheduling.Application.Scheduling.Queries.GetTeachersWithSchedule;
using Scheduling.Domain.Enums;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

// Контроллер ScheduleController группирует HTTP-эндпоинты и делегирует работу в прикладные сценарии.
[ApiController]
[Route("api/schedule")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IMediator _mediator;

    public ScheduleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ---- Учитель: правила расписания ----

    [HttpGet("availability/my")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetMyAvailability(CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var rules = await _mediator.Send(new GetMyAvailabilityQuery(teacherId), ct);
        return Ok(rules);
    }

    [HttpPost("availability")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateAvailability([FromBody] CreateAvailabilityRequest request, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var teacherName = $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}".Trim();
        if (string.IsNullOrEmpty(teacherName))
            teacherName = User.FindFirstValue(ClaimTypes.Name) ?? "Преподаватель";

        var command = new CreateAvailabilityCommand(
            teacherId, teacherName,
            request.Kind, request.DayOfWeek, request.SpecificDate,
            request.StartTime, request.EndTime,
            request.SlotDurationMinutes, request.BreakBetweenMinutes,
            request.ValidFrom, request.ValidUntil,
            request.SessionType, request.MaxStudents,
            request.Title, request.Description, request.MeetingLink,
            request.RequiredCourseId);

        var result = await _mediator.Send(command, ct);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "AVAILABILITY_CREATE_FAILED"));

        return Ok(result.Value);
    }

    [HttpDelete("availability/{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteAvailability(Guid id, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new DeleteAvailabilityCommand(id, teacherId), ct);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "AVAILABILITY_DELETE_FAILED"));
        return Ok(new { message = result.Value });
    }

    // ---- Учитель: материализованные слоты ----

    [HttpGet("slots/my")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetMySlots([FromQuery] string? status, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        SlotStatus? slotStatus = null;
        if (!string.IsNullOrEmpty(status))
        {
            if (status.Equals("completed", StringComparison.OrdinalIgnoreCase)) slotStatus = SlotStatus.Completed;
            else if (status.Equals("cancelled", StringComparison.OrdinalIgnoreCase)) slotStatus = SlotStatus.Cancelled;
            else if (status.Equals("upcoming", StringComparison.OrdinalIgnoreCase)) slotStatus = SlotStatus.Available;
        }

        var slots = await _mediator.Send(new GetTeacherSlotsQuery(teacherId, slotStatus), ct);
        return Ok(slots);
    }

    [HttpPut("slots/{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateSlot(Guid id, [FromBody] UpdateSlotRequest request, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new UpdateSlotCommand(id, teacherId, request.Title, request.Description,
            request.StartTime, request.EndTime, request.MeetingLink, request.MaxStudents);

        var result = await _mediator.Send(command, ct);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "SLOT_UPDATE_FAILED"));
        return Ok(result.Value);
    }

    [HttpPost("slots/{id:guid}/cancel")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CancelSlot(Guid id, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new CancelSlotCommand(id, teacherId), ct);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "SLOT_CANCEL_FAILED"));
        return Ok(new { message = result.Value });
    }

    [HttpPost("slots/{id:guid}/complete")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CompleteSlot(Guid id, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new CompleteSlotCommand(id, teacherId), ct);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "SLOT_COMPLETE_FAILED"));
        return Ok(new { message = result.Value });
    }

    [HttpGet("slots/{id:guid}")]
    public async Task<IActionResult> GetSlotById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSlotByIdQuery(id), ct);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "SLOT_NOT_FOUND"));
        return Ok(result.Value);
    }

    [HttpGet("slots/{id:guid}/bookings")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetSlotBookings(Guid id, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetSlotByIdQuery(id), ct);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "SLOT_NOT_FOUND"));
        if (result.Value!.TeacherId != teacherId)
            return Forbid();
        return Ok(result.Value.Bookings);
    }

    // ---- Студент: календарь учителя и бронирование ----

    [HttpGet("teachers")]
    [Authorize]
    public async Task<IActionResult> GetTeachersWithSchedule(CancellationToken ct)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var teachers = await _mediator.Send(new GetTeachersWithScheduleQuery(currentUserId), ct);
        return Ok(teachers);
    }

    [HttpGet("teachers/{teacherId}/calendar")]
    [Authorize]
    public async Task<IActionResult> GetTeacherCalendar(
        string teacherId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var fromDate = from ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var toDate = to ?? fromDate.AddDays(28);
        var slots = await _mediator.Send(new GetTeacherCalendarQuery(teacherId, fromDate, toDate, currentUserId), ct);
        return Ok(slots);
    }

    [HttpPost("book")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> BookSlot([FromBody] BookSlotRequest request, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var studentName = $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}".Trim();
        if (string.IsNullOrEmpty(studentName))
            studentName = User.FindFirstValue(ClaimTypes.Name) ?? "Студент";

        var result = await _mediator.Send(
            new BookSlotCommand(request.AvailabilityId, request.StartTime, studentId, studentName), ct);

        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "SLOT_BOOK_FAILED"));
        return Ok(new { message = result.Value });
    }

    [HttpDelete("slots/{id:guid}/book")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new CancelBookingCommand(id, studentId), ct);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "BOOKING_CANCEL_FAILED"));
        return Ok(new { message = result.Value });
    }

    [HttpGet("my-bookings")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyBookings(CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var slots = await _mediator.Send(new GetMyBookingsQuery(studentId), ct);
        return Ok(slots);
    }
}

// API-модель CreateAvailabilityRequest фиксирует тело запроса или результат для действия контроллера.
public record CreateAvailabilityRequest(
    AvailabilityKind Kind,
    DayOfWeek? DayOfWeek,
    DateOnly? SpecificDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    int BreakBetweenMinutes,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    SessionType SessionType,
    int MaxStudents,
    string Title,
    string? Description,
    string? MeetingLink,
    Guid? RequiredCourseId);

// API-модель BookSlotRequest фиксирует тело запроса или результат для действия контроллера.
public record BookSlotRequest(Guid AvailabilityId, DateTime StartTime);

public record UpdateSlotRequest(
    string? Title,
    string? Description,
    DateTime? StartTime,
    DateTime? EndTime,
    string? MeetingLink,
    int? MaxStudents);
