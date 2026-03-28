using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;
using Tests.Domain.Enums;

namespace Tests.Application.Tests.Commands.AddQuestion;

public record AnswerOptionInput(string Text, bool IsCorrect, string? MatchingPairValue);

public record AddQuestionCommand(
    Guid TestId,
    string CreatedById,
    QuestionType Type,
    string Text,
    int Points,
    List<AnswerOptionInput> AnswerOptions
) : IRequest<Result<QuestionDto>>;
