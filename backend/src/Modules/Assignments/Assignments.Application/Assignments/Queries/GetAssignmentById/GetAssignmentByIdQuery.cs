// GetAssignmentByIdQuery.cs

using Assignments.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetAssignmentById;

/// <summary>
/// Запрос хранит id задания, по которому нужна подробная карточка со сдачами.
/// </summary>
public record GetAssignmentByIdQuery(Guid Id) : IRequest<Result<AssignmentDetailDto>>;
