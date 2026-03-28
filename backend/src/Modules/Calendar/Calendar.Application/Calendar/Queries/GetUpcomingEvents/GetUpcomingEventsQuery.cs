using Calendar.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Calendar.Application.Calendar.Queries.GetUpcomingEvents;

public record GetUpcomingEventsQuery(string UserId, int Count = 10) : IRequest<Result<List<CalendarEventDto>>>;
