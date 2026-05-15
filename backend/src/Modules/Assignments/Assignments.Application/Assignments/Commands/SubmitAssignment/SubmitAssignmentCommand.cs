// SubmitAssignmentCommand.cs

using Assignments.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.SubmitAssignment;

/// <summary>
/// Команда сдачи хранит id задания, id студента и текстовый ответ; файлы живут отдельно как вложения.
/// </summary>
public record SubmitAssignmentCommand(Guid AssignmentId, string StudentId, string? Content) : IRequest<Result<SubmissionDto>>;
