// GradeSubmissionCommand.cs

using Assignments.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.GradeSubmission;

/// <summary>
/// Команда проверки содержит id сдачи, id преподавателя, балл, комментарий и итоговый статус.
/// </summary>
public record GradeSubmissionCommand(
    Guid SubmissionId,
    string TeacherId,
    int Score,
    string? Comment,
    SubmissionStatus Status) : IRequest<Result<string>>;
