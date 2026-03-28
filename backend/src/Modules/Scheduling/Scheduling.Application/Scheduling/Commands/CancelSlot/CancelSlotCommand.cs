using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.CancelSlot;

public record CancelSlotCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;
