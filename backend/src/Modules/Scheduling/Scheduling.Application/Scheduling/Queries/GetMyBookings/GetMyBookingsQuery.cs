using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetMyBookings;

public record GetMyBookingsQuery(string StudentId) : IRequest<List<ScheduleSlotDto>>;
