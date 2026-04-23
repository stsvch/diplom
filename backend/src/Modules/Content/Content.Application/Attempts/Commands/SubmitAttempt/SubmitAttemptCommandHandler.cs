using Content.Application.DTOs;
using Content.Application.Grading;
using Content.Application.Interfaces;
using Content.Domain.Entities;
using Content.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Attempts.Commands.SubmitAttempt;

public class SubmitAttemptCommandHandler : IRequestHandler<SubmitAttemptCommand, Result<SubmitAttemptResultDto>>
{
    private readonly IContentDbContext _context;
    private readonly IBlockGraderRegistry _graderRegistry;
    private readonly ILessonProgressUpdater _progressUpdater;

    public SubmitAttemptCommandHandler(
        IContentDbContext context,
        IBlockGraderRegistry graderRegistry,
        ILessonProgressUpdater progressUpdater)
    {
        _context = context;
        _graderRegistry = graderRegistry;
        _progressUpdater = progressUpdater;
    }

    public async Task<Result<SubmitAttemptResultDto>> Handle(SubmitAttemptCommand request, CancellationToken cancellationToken)
    {
        var block = await _context.LessonBlocks.FindAsync([request.BlockId], cancellationToken);
        if (block is null)
            return Result.Failure<SubmitAttemptResultDto>("Блок не найден.");

        if (request.Answers.Type != block.Type)
            return Result.Failure<SubmitAttemptResultDto>("Тип ответа не соответствует типу блока.");

        var attempt = await _context.LessonBlockAttempts
            .FirstOrDefaultAsync(a => a.BlockId == request.BlockId && a.UserId == request.UserId, cancellationToken);

        if (attempt is not null && block.Settings.MaxAttempts.HasValue &&
            attempt.AttemptsUsed >= block.Settings.MaxAttempts.Value)
        {
            return Result.Failure<SubmitAttemptResultDto>("Лимит попыток исчерпан.");
        }

        var grade = _graderRegistry.Grade(block.Type, block.Data, request.Answers, block.Settings);
        var now = DateTime.UtcNow;

        if (attempt is null)
        {
            attempt = new LessonBlockAttempt
            {
                BlockId = request.BlockId,
                UserId = request.UserId,
                Answers = request.Answers,
                Score = grade.Score,
                MaxScore = grade.MaxScore,
                IsCorrect = grade.IsCorrect,
                NeedsReview = grade.NeedsReview,
                AttemptsUsed = 1,
                Status = grade.NeedsReview ? LessonBlockAttemptStatus.Submitted : LessonBlockAttemptStatus.Graded,
                SubmittedAt = now
            };
            _context.LessonBlockAttempts.Add(attempt);
        }
        else
        {
            attempt.Answers = request.Answers;
            attempt.Score = grade.Score;
            attempt.MaxScore = grade.MaxScore;
            attempt.IsCorrect = grade.IsCorrect;
            attempt.NeedsReview = grade.NeedsReview;
            attempt.AttemptsUsed += 1;
            attempt.Status = grade.NeedsReview ? LessonBlockAttemptStatus.Submitted : LessonBlockAttemptStatus.Graded;
            attempt.SubmittedAt = now;
            attempt.ReviewedAt = null;
            attempt.ReviewerId = null;
            attempt.ReviewerComment = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await TryMarkLessonCompletedAsync(block.LessonId, request.UserId, cancellationToken);

        int? attemptsRemaining = block.Settings.MaxAttempts.HasValue
            ? Math.Max(0, block.Settings.MaxAttempts.Value - attempt.AttemptsUsed)
            : null;

        return Result.Success(new SubmitAttemptResultDto
        {
            AttemptId = attempt.Id,
            Score = attempt.Score,
            MaxScore = attempt.MaxScore,
            IsCorrect = attempt.IsCorrect,
            NeedsReview = attempt.NeedsReview,
            AttemptsUsed = attempt.AttemptsUsed,
            AttemptsRemaining = attemptsRemaining,
            Feedback = grade.Feedback
        });
    }

    private async Task TryMarkLessonCompletedAsync(Guid lessonId, Guid userId, CancellationToken cancellationToken)
    {
        var requiredBlocks = await _context.LessonBlocks
            .Where(b => b.LessonId == lessonId)
            .ToListAsync(cancellationToken);

        var required = requiredBlocks.Where(b => b.Settings.RequiredForCompletion).ToList();
        if (required.Count == 0) return;

        var requiredIds = required.Select(b => b.Id).ToList();
        var passedCount = await _context.LessonBlockAttempts
            .Where(a => requiredIds.Contains(a.BlockId) && a.UserId == userId)
            .CountAsync(a => a.IsCorrect || (a.Status == LessonBlockAttemptStatus.Graded && a.Score > 0), cancellationToken);

        if (passedCount == required.Count)
        {
            await _progressUpdater.MarkLessonCompletedAsync(lessonId, userId, cancellationToken);
        }
    }
}
