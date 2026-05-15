// GetMyBookingsQuery.cs

using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetMyBookings;

/// <summary>
/// CQRS-запрос GetMyBookingsQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetMyBookingsQuery(string StudentId) : IRequest<List<ScheduleSlotDto>>;
