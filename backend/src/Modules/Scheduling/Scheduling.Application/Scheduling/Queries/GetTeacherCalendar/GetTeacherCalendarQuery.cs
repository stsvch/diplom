// GetTeacherCalendarQuery.cs

using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetTeacherCalendar;

/// <summary>
/// CQRS-запрос GetTeacherCalendarQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetTeacherCalendarQuery(
    string TeacherId,
    DateOnly From,
    DateOnly To,
    string? CurrentUserId
) : IRequest<List<CalendarSlotDto>>;
