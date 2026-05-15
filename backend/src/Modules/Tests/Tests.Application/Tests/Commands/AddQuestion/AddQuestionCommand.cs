// AddQuestionCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;
using Tests.Domain.Enums;

namespace Tests.Application.Tests.Commands.AddQuestion;

/// <summary>
/// CQRS-команда AnswerOptionInput описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record AnswerOptionInput(string Text, bool IsCorrect, string? MatchingPairValue);

/// CQRS-команда AddQuestionCommand описывает входные данные операции, которая меняет состояние модуля.
public record AddQuestionCommand(
    Guid TestId,
    string CreatedById,
    QuestionType Type,
    string Text,
    int Points,
    List<AnswerOptionInput> AnswerOptions,
    QuestionGradeType GradeType = QuestionGradeType.Auto,
    string? Explanation = null,
    string? ExpectedAnswer = null
) : IRequest<Result<QuestionDto>>;
