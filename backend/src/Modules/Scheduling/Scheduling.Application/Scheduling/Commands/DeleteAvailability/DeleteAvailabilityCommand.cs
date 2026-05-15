// DeleteAvailabilityCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.DeleteAvailability;

/// <summary>
/// CQRS-команда DeleteAvailabilityCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record DeleteAvailabilityCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;
