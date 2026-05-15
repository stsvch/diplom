// GetUpcomingEventsQuery.cs
using Calendar.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Calendar.Application.Calendar.Queries.GetUpcomingEvents;

// Query описывает параметры чтения без изменения состояния.
public record GetUpcomingEventsQuery(string UserId, int Count = 10) : IRequest<Result<List<CalendarEventDto>>>;
