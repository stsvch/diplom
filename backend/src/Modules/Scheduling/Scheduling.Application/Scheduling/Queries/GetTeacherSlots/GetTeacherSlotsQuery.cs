// GetTeacherSlotsQuery.cs

using MediatR;
using Scheduling.Application.DTOs;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Queries.GetTeacherSlots;

/// <summary>
/// CQRS-запрос GetTeacherSlotsQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetTeacherSlotsQuery(string TeacherId, SlotStatus? Status) : IRequest<List<ScheduleSlotDto>>;
