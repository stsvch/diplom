// CreateAssignmentCommand.cs

using Assignments.Application.DTOs;
using Assignments.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.CreateAssignment;

/// <summary>
/// Команда создания задания хранит курс, текст задания, дедлайн, лимит попыток, максимальный балл, автора, формат сдачи и критерии.
/// </summary>
public record CreateAssignmentCommand(
    Guid CourseId,
    string Title,
    string Description,
    string? Criteria,
    DateTime? Deadline,
    int? MaxAttempts,
    int MaxScore,
    string CreatedById,
    AssignmentSubmissionFormat SubmissionFormat = AssignmentSubmissionFormat.Both,
    IReadOnlyList<AssignmentCriteriaInput>? CriteriaItems = null
) : IRequest<Result<AssignmentDto>>;
