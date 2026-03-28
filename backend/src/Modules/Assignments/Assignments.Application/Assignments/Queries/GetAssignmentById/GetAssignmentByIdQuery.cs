using Assignments.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetAssignmentById;

public record GetAssignmentByIdQuery(Guid Id) : IRequest<Result<AssignmentDetailDto>>;
