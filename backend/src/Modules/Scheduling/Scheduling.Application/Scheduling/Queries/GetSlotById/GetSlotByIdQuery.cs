using EduPlatform.Shared.Domain;
using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetSlotById;

public record GetSlotByIdQuery(Guid Id) : IRequest<Result<ScheduleSlotDto>>;
