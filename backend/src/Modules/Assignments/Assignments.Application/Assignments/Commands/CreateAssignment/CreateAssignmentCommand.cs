using Assignments.Application.DTOs;
using Assignments.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.CreateAssignment;

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
