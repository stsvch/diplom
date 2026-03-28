using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(string StudentId) : IRequest<List<ScheduleSlotDto>>;
