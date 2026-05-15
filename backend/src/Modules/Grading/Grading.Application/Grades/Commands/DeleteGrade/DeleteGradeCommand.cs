// DeleteGradeCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Grading.Application.Grades.Commands.DeleteGrade;

/// <summary>
/// CQRS-команда DeleteGradeCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record DeleteGradeCommand(Guid Id, string RequesterId) : IRequest<Result>;
