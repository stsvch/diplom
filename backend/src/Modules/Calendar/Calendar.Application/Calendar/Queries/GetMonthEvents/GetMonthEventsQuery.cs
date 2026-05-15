// GetMonthEventsQuery.cs
using Calendar.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Calendar.Application.Calendar.Queries.GetMonthEvents;

// Query описывает параметры чтения без изменения состояния.
public record GetMonthEventsQuery(string UserId, int Year, int Month) : IRequest<Result<List<CalendarEventDto>>>;
