using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Tests.Commands.GradeResponse;

public record GradeResponseCommand(
    Guid ResponseId,
    string TeacherId,
    int Points,
    string? Comment
) : IRequest<Result<string>>;
