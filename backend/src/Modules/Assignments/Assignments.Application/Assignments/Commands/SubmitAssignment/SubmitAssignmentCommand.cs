using Assignments.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.SubmitAssignment;

public record SubmitAssignmentCommand(Guid AssignmentId, string StudentId, string? Content) : IRequest<Result<SubmissionDto>>;
