using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.BookSlot;

public record BookSlotCommand(Guid SlotId, string StudentId, string StudentName) : IRequest<Result<string>>;
