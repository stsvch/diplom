using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.CompleteSlot;

public record CompleteSlotCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;
