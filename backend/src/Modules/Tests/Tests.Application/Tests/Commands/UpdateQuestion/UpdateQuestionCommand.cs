// UpdateQuestionCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;
using Tests.Domain.Enums;

namespace Tests.Application.Tests.Commands.UpdateQuestion;

/// <summary>
/// CQRS-команда UpdateAnswerOptionInput описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record UpdateAnswerOptionInput(Guid? Id, string Text, bool IsCorrect, string? MatchingPairValue);

/// CQRS-команда UpdateQuestionCommand описывает входные данные операции, которая меняет состояние модуля.
public record UpdateQuestionCommand(
    Guid Id,
    string CreatedById,
    QuestionType Type,
    string Text,
    int Points,
    List<UpdateAnswerOptionInput> AnswerOptions,
    QuestionGradeType? GradeType = null,
    string? Explanation = null,
    string? ExpectedAnswer = null
) : IRequest<Result<QuestionDto>>;
