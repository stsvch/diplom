using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.DeleteAssignment;

public record DeleteAssignmentCommand(Guid Id, string CreatedById) : IRequest<Result<string>>;
