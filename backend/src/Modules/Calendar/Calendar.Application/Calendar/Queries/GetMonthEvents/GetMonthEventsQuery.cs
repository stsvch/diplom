using Calendar.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Calendar.Application.Calendar.Queries.GetMonthEvents;

public record GetMonthEventsQuery(string UserId, int Year, int Month) : IRequest<Result<List<CalendarEventDto>>>;
