using Assignments.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.GradeSubmission;

public record GradeSubmissionCommand(
    Guid SubmissionId,
    string TeacherId,
    int Score,
    string? Comment,
    SubmissionStatus Status) : IRequest<Result<string>>;
