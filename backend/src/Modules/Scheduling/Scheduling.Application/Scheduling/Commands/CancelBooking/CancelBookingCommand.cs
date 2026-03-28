using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.CancelBooking;

public record CancelBookingCommand(Guid SlotId, string StudentId) : IRequest<Result<string>>;
