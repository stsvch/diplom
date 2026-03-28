using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Commands.CreateTest;

public record CreateTestCommand(
    string Title,
    string? Description,
    string CreatedById,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    bool ShuffleQuestions,
    bool ShuffleAnswers,
    bool ShowCorrectAnswers
) : IRequest<Result<TestDetailDto>>;
