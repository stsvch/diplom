// UpdateGradeCommand.cs

using EduPlatform.Shared.Domain;
using Grading.Application.DTOs;
using MediatR;

namespace Grading.Application.Grades.Commands.UpdateGrade;

/// <summary>
/// CQRS-команда UpdateGradeCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record UpdateGradeCommand(
    Guid Id,
    decimal Score,
    decimal MaxScore,
    string? Comment
) : IRequest<Result<GradeDto>>;
