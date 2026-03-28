using EduPlatform.Shared.Domain;
using Grading.Application.DTOs;
using Grading.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Grading.Application.Grades.Commands.UpdateGrade;

public class UpdateGradeCommandHandler : IRequestHandler<UpdateGradeCommand, Result<GradeDto>>
{
    private readonly IGradingDbContext _context;

    public UpdateGradeCommandHandler(IGradingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GradeDto>> Handle(UpdateGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (grade is null)
            return Result.Failure<GradeDto>("Grade not found");

        grade.Score = request.Score;
        grade.MaxScore = request.MaxScore;
        grade.Comment = request.Comment;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new GradeDto
        {
            Id = grade.Id,
            StudentId = grade.StudentId,
            CourseId = grade.CourseId,
            SourceType = grade.SourceType,
            Title = grade.Title,
            Score = grade.Score,
            MaxScore = grade.MaxScore,
            Comment = grade.Comment,
            GradedAt = grade.GradedAt
        });
    }
}
