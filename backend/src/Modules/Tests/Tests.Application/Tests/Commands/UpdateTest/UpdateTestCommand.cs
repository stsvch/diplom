// UpdateTestCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Commands.UpdateTest;

/// <summary>
/// CQRS-команда UpdateTestCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record UpdateTestCommand(
    Guid Id,
    string CreatedById,
    Guid CourseId,
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    DateTime? Deadline,
    bool ShuffleQuestions,
    bool ShuffleAnswers,
    bool ShowCorrectAnswers
) : IRequest<Result<TestDetailDto>>;
