using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetTeacherCalendar;

public record GetTeacherCalendarQuery(
    string TeacherId,
    DateOnly From,
    DateOnly To,
    string? CurrentUserId
) : IRequest<List<CalendarSlotDto>>;
