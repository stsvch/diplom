using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Commands.CreateTest;

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
