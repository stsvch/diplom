using Calendar.Application.Calendar.Commands.CreateCalendarEvent;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifications.Application.Notifications.Commands.CreateNotification;
using Scheduling.Application.Scheduling.Commands.BookSlot;
using Scheduling.Application.Scheduling.Commands.CancelBooking;
using Scheduling.Application.Scheduling.Commands.CancelSlot;
using Scheduling.Application.Scheduling.Commands.CompleteSlot;
using Scheduling.Application.Scheduling.Commands.CreateSlot;
using Scheduling.Application.Scheduling.Commands.UpdateSlot;
using Scheduling.Application.Scheduling.Queries.GetAvailableSlots;
using Scheduling.Application.Scheduling.Queries.GetMyBookings;
using Scheduling.Application.Scheduling.Queries.GetSlotById;
using Scheduling.Application.Scheduling.Queries.GetTeacherSlots;
using Scheduling.Domain.Enums;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

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

    // ---- Teacher endpoints ----

    [HttpPost("slots")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSlotRequest request, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var teacherName = $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}".Trim();
        if (string.IsNullOrEmpty(teacherName))
            teacherName = User.FindFirstValue(ClaimTypes.Name) ?? "Преподаватель";

        var command = new CreateSlotCommand(
            teacherId, teacherName,
            request.CourseId, request.CourseName,
            request.Title, request.Description,
            request.StartTime, request.EndTime,
            request.IsGroupSession, request.MaxStudents,
            request.MeetingLink);

        var result = await _mediator.Send(command, ct);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "SLOT_CREATE_FAILED"));

        // Create calendar event for the teacher
        try
        {
            await _mediator.Send(new CreateCalendarEventCommand(
                teacherId,
                request.CourseId,
                result.Value!.Title,
                request.Description,
                request.StartTime.Date,
                request.StartTime.ToString("HH:mm"),
                CalendarEventType.Lesson,
                "ScheduleSlot",
                result.Value.Id), ct);
        }
        catch
        {
            // Calendar event creation is optional
        }

        return Ok(result.Value);
    }

    [HttpGet("slots/my")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetMySlots([FromQuery] string? status, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        SlotStatus? slotStatus = null;

        if (!string.IsNullOrEmpty(status))
        {
            if (status.Equals("completed", StringComparison.OrdinalIgnoreCase))
                slotStatus = SlotStatus.Completed;
            else if (status.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
                slotStatus = SlotStatus.Cancelled;
            else if (status.Equals("upcoming", StringComparison.OrdinalIgnoreCase))
                slotStatus = SlotStatus.Available;
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

    // ---- Student endpoints ----

    [HttpGet("available")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetAvailableSlots(CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var slots = await _mediator.Send(new GetAvailableSlotsQuery(studentId), ct);
        return Ok(slots);
    }

    [HttpPost("slots/{id:guid}/book")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> BookSlot(Guid id, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var studentName = $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}".Trim();
        if (string.IsNullOrEmpty(studentName))
            studentName = User.FindFirstValue(ClaimTypes.Name) ?? "Студент";

        var result = await _mediator.Send(new BookSlotCommand(id, studentId, studentName), ct);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "SLOT_BOOK_FAILED"));

        // Send notification to teacher
        try
        {
            var slotResult = await _mediator.Send(new GetSlotByIdQuery(id), ct);
            if (slotResult.IsSuccess)
            {
                await _mediator.Send(new CreateNotificationCommand(
                    slotResult.Value!.TeacherId,
                    NotificationType.Course,
                    "Новая запись на занятие",
                    $"{studentName} записался(-ась) на занятие «{slotResult.Value.Title}».",
                    $"/teacher/schedule"), ct);
            }
        }
        catch
        {
            // Notification is optional
        }

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

public record CreateSlotRequest(
    Guid? CourseId,
    string? CourseName,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    bool IsGroupSession,
    int MaxStudents,
    string? MeetingLink);

public record UpdateSlotRequest(
    string? Title,
    string? Description,
    DateTime? StartTime,
    DateTime? EndTime,
    string? MeetingLink,
    int? MaxStudents);
