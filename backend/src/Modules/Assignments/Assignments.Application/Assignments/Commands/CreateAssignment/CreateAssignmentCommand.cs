using Assignments.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.CreateAssignment;

public record CreateAssignmentCommand(
    string Title,
    string Description,
    string? Criteria,
    DateTime? Deadline,
    int? MaxAttempts,
    int MaxScore,
    string CreatedById
) : IRequest<Result<AssignmentDto>>;
