// DeleteAssignmentCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.DeleteAssignment;

/// <summary>
/// Команда удаления содержит id задания и id автора, чтобы удалить только своё задание.
/// </summary>
public record DeleteAssignmentCommand(Guid Id, string CreatedById) : IRequest<Result<string>>;
