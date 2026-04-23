using Calendar.Application.DTOs;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;

namespace Calendar.Application.Calendar.Commands.CreateCalendarEvent;

public record CreateCalendarEventCommand(
    string? UserId,
    Guid? CourseId,
    string Title,
    string? Description,
    DateTime EventDate,
    string? EventTime,
    CalendarEventType Type,
    string? SourceType,
    Guid? SourceId
) : IRequest<Result<CalendarEventDto>>;
