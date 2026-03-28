using EduPlatform.Shared.Domain;
using Grading.Application.DTOs;
using MediatR;

namespace Grading.Application.Grades.Commands.UpdateGrade;

public record UpdateGradeCommand(
    Guid Id,
    decimal Score,
    decimal MaxScore,
    string? Comment
) : IRequest<Result<GradeDto>>;
