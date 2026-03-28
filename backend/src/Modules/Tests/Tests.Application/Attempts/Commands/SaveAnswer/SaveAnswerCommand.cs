using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Attempts.Commands.SaveAnswer;

public record SaveAnswerCommand(
    Guid AttemptId,
    string StudentId,
    Guid QuestionId,
    List<string>? SelectedOptionIds,
    string? TextAnswer
) : IRequest<Result<string>>;
