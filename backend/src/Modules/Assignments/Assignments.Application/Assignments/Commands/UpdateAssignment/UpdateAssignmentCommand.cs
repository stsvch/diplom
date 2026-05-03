using Assignments.Application.DTOs;
using Assignments.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.UpdateAssignment;

public record UpdateAssignmentCommand(
    Guid Id,
    string CreatedById,
    Guid CourseId,
    string Title,
    string Description,
    string? Criteria,
    DateTime? Deadline,
    int? MaxAttempts,
    int MaxScore,
    AssignmentSubmissionFormat? SubmissionFormat = null,
    /// <summary>Если задан — полностью заменяет CriteriaItems. Null — оставить как есть.</summary>
    IReadOnlyList<AssignmentCriteriaInput>? CriteriaItems = null
) : IRequest<Result<AssignmentDto>>;
