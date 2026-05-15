// StartAttemptCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Attempts.Commands.StartAttempt;

/// <summary>
/// CQRS-команда StartAttemptCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record StartAttemptCommand(Guid TestId, string StudentId) : IRequest<Result<TestAttemptStartDto>>;
