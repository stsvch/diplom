using Content.Application.DTOs;
using Content.Application.Interfaces;
using Content.Domain.Entities;
using Content.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Attempts.Queries.GetLessonProgress;

public class GetLessonProgressQueryHandler : IRequestHandler<GetLessonProgressQuery, LessonProgressDto>
{
    private readonly IContentDbContext _context;

    public GetLessonProgressQueryHandler(IContentDbContext context)
    {
        _context = context;
    }

    public async Task<LessonProgressDto> Handle(GetLessonProgressQuery request, CancellationToken cancellationToken)
    {
        var blocks = await _context.LessonBlocks
            .AsNoTracking()
            .Where(b => b.LessonId == request.LessonId)
            .ToListAsync(cancellationToken);

        if (blocks.Count == 0)
        {
            return new LessonProgressDto { LessonId = request.LessonId, IsCompleted = false };
        }

        var blockIds = blocks.Select(b => b.Id).ToList();
        var attempts = await _context.LessonBlockAttempts
            .AsNoTracking()
            .Where(a => blockIds.Contains(a.BlockId) && a.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var requiredBlocks = blocks.Where(b => b.Settings.RequiredForCompletion).ToList();
        var totalScore = attempts.Sum(a => a.Score);
        var maxScore = blocks.Sum(b => b.Settings.Points);

        var completedBlocks = requiredBlocks
            .Count(b => attempts.Any(a => a.BlockId == b.Id && IsBlockPassed(a)));

        var isCompleted = requiredBlocks.Count > 0 && completedBlocks == requiredBlocks.Count;

        return new LessonProgressDto
        {
            LessonId = request.LessonId,
            TotalBlocks = blocks.Count,
            RequiredBlocks = requiredBlocks.Count,
            CompletedBlocks = completedBlocks,
            TotalScore = totalScore,
            MaxScore = maxScore,
            Percentage = maxScore > 0 ? Math.Round(totalScore / maxScore * 100m, 2) : 0m,
            IsCompleted = isCompleted
        };
    }

    private static bool IsBlockPassed(LessonBlockAttempt a)
    {
        if (a.IsCorrect) return true;
        if (a.Status == LessonBlockAttemptStatus.Graded && a.Score > 0) return true;
        return false;
    }
}
