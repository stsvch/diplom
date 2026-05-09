using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.DeleteAvailability;

public record DeleteAvailabilityCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;
