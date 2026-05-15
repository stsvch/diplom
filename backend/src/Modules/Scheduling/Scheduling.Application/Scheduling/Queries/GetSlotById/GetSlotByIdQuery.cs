// GetSlotByIdQuery.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetSlotById;

/// <summary>
/// CQRS-запрос GetSlotByIdQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetSlotByIdQuery(Guid Id) : IRequest<Result<ScheduleSlotDto>>;
