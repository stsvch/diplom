// SaveAnswerCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Attempts.Commands.SaveAnswer;

/// <summary>
/// CQRS-команда SaveAnswerCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record SaveAnswerCommand(
    Guid AttemptId,
    string StudentId,
    Guid QuestionId,
    List<string>? SelectedOptionIds,
    string? TextAnswer
) : IRequest<Result<string>>;
