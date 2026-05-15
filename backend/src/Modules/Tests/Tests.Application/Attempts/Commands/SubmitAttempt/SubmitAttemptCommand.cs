// SubmitAttemptCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Attempts.Commands.SubmitAttempt;

/// <summary>
/// CQRS-команда SubmitAttemptCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record SubmitAttemptCommand(Guid AttemptId, string StudentId) : IRequest<Result<TestAttemptDetailDto>>;
