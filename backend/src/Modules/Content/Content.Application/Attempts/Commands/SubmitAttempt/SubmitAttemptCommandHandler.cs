using Content.Application.DTOs;
using Content.Application.Grading;
using Content.Application.Interfaces;
using Content.Application.CodeExecution;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.Entities;
using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;
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
    private readonly ICodeExecutor _codeExecutor;

    public SubmitAttemptCommandHandler(
        IContentDbContext context,
        IBlockGraderRegistry graderRegistry,
        ILessonProgressUpdater progressUpdater,
        ICodeExecutor codeExecutor)
    {
        _context = context;
        _graderRegistry = graderRegistry;
        _progressUpdater = progressUpdater;
        _codeExecutor = codeExecutor;
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

        var preparedAnswersResult = await PrepareAnswersAsync(block.Type, block.Data, request.Answers, cancellationToken);
        if (preparedAnswersResult.IsFailure)
            return Result.Failure<SubmitAttemptResultDto>(preparedAnswersResult.Error!);

        var preparedAnswers = preparedAnswersResult.Value!;
        var grade = _graderRegistry.Grade(block.Type, block.Data, preparedAnswers, block.Settings);
        var now = DateTime.UtcNow;

        if (attempt is null)
        {
            attempt = new LessonBlockAttempt
            {
                BlockId = request.BlockId,
                UserId = request.UserId,
                Answers = preparedAnswers,
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
            attempt.Answers = preparedAnswers;
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

        if (block.Type == LessonBlockType.CodeExercise
            && block.Data is CodeExerciseBlockData codeData
            && preparedAnswers is CodeExerciseAnswer codeAnswer)
        {
            _context.CodeExerciseRuns.Add(new CodeExerciseRun
            {
                BlockId = block.Id,
                UserId = request.UserId,
                AttemptId = attempt.Id,
                Kind = CodeExerciseRunKind.Submission,
                Language = codeData.Language,
                Code = codeAnswer.Code,
                Ok = true,
                GlobalError = null,
                Results = codeAnswer.RunOutput?.Select(r => new CodeTestCaseResult
                {
                    Input = r.Input,
                    ExpectedOutput = r.ExpectedOutput,
                    ActualOutput = r.ActualOutput,
                    Passed = r.Passed,
                    IsHidden = r.IsHidden
                }).ToList() ?? new List<CodeTestCaseResult>(),
                CreatedAt = now
            });
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

    private async Task<Result<LessonBlockAnswer>> PrepareAnswersAsync(
        LessonBlockType blockType,
        LessonBlockData blockData,
        LessonBlockAnswer submittedAnswers,
        CancellationToken cancellationToken)
    {
        if (blockType != LessonBlockType.CodeExercise
            || blockData is not CodeExerciseBlockData codeData
            || submittedAnswers is not CodeExerciseAnswer codeAnswer)
        {
            return Result.Success(submittedAnswers);
        }

        if (string.IsNullOrWhiteSpace(codeAnswer.Code))
            return Result.Failure<LessonBlockAnswer>("Код не может быть пустым.");

        var cases = codeData.TestCases
            .Select(t => new CodeExecutionCase(t.Input, t.ExpectedOutput, t.IsHidden))
            .ToList();

        var executionRequest = new CodeExecutionRequest(
            codeData.Language,
            codeAnswer.Code,
            cases,
            codeData.TimeoutMs,
            codeData.MemoryLimitMb);

        var executionResponse = await _codeExecutor.ExecuteAsync(executionRequest, cancellationToken);
        if (!executionResponse.Ok)
            return Result.Failure<LessonBlockAnswer>(executionResponse.GlobalError ?? "Не удалось проверить код.");

        return Result.Success<LessonBlockAnswer>(
            CodeExerciseSanitizer.BuildStoredAnswer(codeData, codeAnswer.Code, executionResponse.Results));
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
