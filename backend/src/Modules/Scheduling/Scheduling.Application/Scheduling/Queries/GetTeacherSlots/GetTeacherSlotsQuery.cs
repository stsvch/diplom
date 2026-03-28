using MediatR;
using Scheduling.Application.DTOs;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Queries.GetTeacherSlots;

public record GetTeacherSlotsQuery(string TeacherId, SlotStatus? Status) : IRequest<List<ScheduleSlotDto>>;
