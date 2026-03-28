using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;
using Tests.Domain.Enums;

namespace Tests.Application.Tests.Commands.UpdateQuestion;

public record UpdateAnswerOptionInput(Guid? Id, string Text, bool IsCorrect, string? MatchingPairValue);

public record UpdateQuestionCommand(
    Guid Id,
    string CreatedById,
    QuestionType Type,
    string Text,
    int Points,
    List<UpdateAnswerOptionInput> AnswerOptions
) : IRequest<Result<QuestionDto>>;
