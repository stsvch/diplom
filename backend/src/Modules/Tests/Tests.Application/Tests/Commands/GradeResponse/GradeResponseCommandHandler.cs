using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;
using Tests.Domain.Enums;

namespace Tests.Application.Tests.Commands.GradeResponse;

public class GradeResponseCommandHandler : IRequestHandler<GradeResponseCommand, Result<string>>
{
    private readonly ITestsDbContext _context;

    public GradeResponseCommandHandler(ITestsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(GradeResponseCommand request, CancellationToken cancellationToken)
    {
        var response = await _context.TestResponses
            .Include(r => r.Attempt)
                .ThenInclude(a => a.Test)
            .Include(r => r.Question)
            .FirstOrDefaultAsync(r => r.Id == request.ResponseId, cancellationToken);

        if (response is null)
            return Result.Failure<string>("Ответ не найден.");

        if (response.Attempt.Test.CreatedById != request.TeacherId)
            return Result.Failure<string>("Вы не являетесь автором этого теста.");

        if (request.Points < 0 || request.Points > response.Question.Points)
            return Result.Failure<string>($"Баллы должны быть от 0 до {response.Question.Points}.");

        response.Points = request.Points;
        response.IsCorrect = request.Points > 0;
        response.TeacherComment = request.Comment;

        // Recalculate attempt score
        var attempt = response.Attempt;
        var allResponses = await _context.TestResponses
            .Where(r => r.AttemptId == attempt.Id)
            .ToListAsync(cancellationToken);

        attempt.Score = allResponses.Sum(r => r.Points ?? 0);

        // Check if all OpenAnswer responses are graded
        var hasUngraded = allResponses.Any(r => r.IsCorrect == null);
        if (!hasUngraded && attempt.Status == AttemptStatus.NeedsReview)
        {
            attempt.Status = AttemptStatus.Completed;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Ответ оценён.");
    }
}
