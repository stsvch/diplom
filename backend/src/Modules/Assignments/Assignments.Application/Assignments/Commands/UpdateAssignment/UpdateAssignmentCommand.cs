using Assignments.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.UpdateAssignment;

public record UpdateAssignmentCommand(
    Guid Id,
    string CreatedById,
    string Title,
    string Description,
    string? Criteria,
    DateTime? Deadline,
    int? MaxAttempts,
    int MaxScore
) : IRequest<Result<AssignmentDto>>;
