using EduPlatform.Shared.Domain;
using Grading.Application.DTOs;
using Grading.Domain.Enums;
using MediatR;

namespace Grading.Application.Grades.Commands.CreateGrade;

public record CreateGradeCommand(
    string StudentId,
    Guid CourseId,
    GradeSourceType SourceType,
    Guid? TestAttemptId,
    Guid? AssignmentSubmissionId,
    string Title,
    decimal Score,
    decimal MaxScore,
    string? Comment,
    DateTime GradedAt,
    string? GradedById
) : IRequest<Result<GradeDto>>;
