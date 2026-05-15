// CreateTestCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Commands.CreateTest;

/// <summary>
/// CQRS-команда CreateTestCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record CreateTestCommand(
    Guid CourseId,
    string Title,
    string? Description,
    string CreatedById,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    DateTime? Deadline,
    bool ShuffleQuestions,
    bool ShuffleAnswers,
    bool ShowCorrectAnswers
) : IRequest<Result<TestDetailDto>>;
