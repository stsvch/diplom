using Content.Application.Attempts.Commands.SubmitAttempt;
using Content.Application.Attempts.Queries.GetMyAttempt;
using Content.Application.CodeExecution;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using Content.Application.LessonBlocks.Commands.CreateLessonBlock;
using Content.Application.LessonBlocks.Commands.DeleteLessonBlock;
using Content.Application.LessonBlocks.Commands.ReorderBlocks;
using Content.Application.LessonBlocks.Commands.UpdateLessonBlock;
using Content.Application.LessonBlocks.Queries.GetLessonBlocks;
using Content.Domain.Entities;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Host.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/lesson-blocks")]
public class LessonBlocksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IContentDbContext _contentDb;
    private readonly LessonAccessService _lessonAccess;

    public LessonBlocksController(IMediator mediator, IContentDbContext contentDb, LessonAccessService lessonAccess)
    {
        _mediator = mediator;
        _contentDb = contentDb;
        _lessonAccess = lessonAccess;
    }

    [HttpGet("by-lesson/{lessonId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(List<LessonBlockDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        if (IsAdmin())
        {
            var adminResult = await _mediator.Send(new GetLessonBlocksQuery(lessonId), cancellationToken);
            return Ok(adminResult);
        }

        if (IsTeacher())
        {
            var canManage = await _lessonAccess.CanTeacherManageLessonAsync(lessonId, userId, cancellationToken);
            if (!canManage)
                return Forbid();

            var teacherResult = await _mediator.Send(new GetLessonBlocksQuery(lessonId), cancellationToken);
            return Ok(teacherResult);
        }

        if (!IsStudent())
            return Forbid();

        var canView = await _lessonAccess.CanStudentAccessLessonAsync(lessonId, userId, cancellationToken);
        if (!canView)
            return Forbid();

        var result = await _mediator.Send(new GetLessonBlocksQuery(lessonId), cancellationToken);
        return Ok(result.Select(SanitizeBlockForStudent).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(LessonBlockDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLessonBlockCommand command, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageLessonAsync(command.LessonId, userId, cancellationToken);
        if (!canManage)
            return Forbid();

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "BLOCK_CREATE_FAILED"));

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(LessonBlockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonBlockRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageBlockAsync(id, userId, cancellationToken);
        if (!canManage)
            return Forbid();

        var command = new UpdateLessonBlockCommand(id, request.Data, request.Settings);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "BLOCK_NOT_FOUND"));

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageBlockAsync(id, userId, cancellationToken);
        if (!canManage)
            return Forbid();

        var result = await _mediator.Send(new DeleteLessonBlockCommand(id), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "BLOCK_NOT_FOUND"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("reorder")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reorder([FromBody] ReorderBlocksCommand command, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageLessonAsync(command.LessonId, userId, cancellationToken);
        if (!canManage)
            return Forbid();

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "BLOCK_REORDER_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("{id:guid}/attempts")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(SubmitAttemptResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitAttempt(Guid id, [FromBody] SubmitAttemptRequest request, CancellationToken cancellationToken)
    {
        var userIdRaw = GetCurrentUserIdRaw();
        var userId = GetCurrentUserId();
        if (userId is null || userIdRaw is null)
            return Unauthorized();

        var canAccess = await _lessonAccess.CanStudentAccessBlockAsync(id, userIdRaw, cancellationToken);
        if (!canAccess)
            return Forbid();

        var command = new SubmitAttemptCommand(id, userId.Value, request.Answers);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "ATTEMPT_SUBMIT_FAILED"));

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/my-attempt")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(LessonBlockAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetMyAttempt(Guid id, CancellationToken cancellationToken)
    {
        var userIdRaw = GetCurrentUserIdRaw();
        var userId = GetCurrentUserId();
        if (userId is null || userIdRaw is null)
            return Unauthorized();

        var canAccess = await _lessonAccess.CanStudentAccessBlockAsync(id, userIdRaw, cancellationToken);
        if (!canAccess)
            return Forbid();

        var result = await _mediator.Send(new GetMyAttemptQuery(id, userId.Value), cancellationToken);
        if (result is null)
            return NoContent();

        var codeData = await LoadCodeExerciseDataAsync(id, cancellationToken);
        return Ok(SanitizeAttemptForStudent(result, codeData));
    }

    [HttpGet("{id:guid}/my-code-runs")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(List<CodeExerciseRunDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCodeRuns(Guid id, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var userIdRaw = GetCurrentUserIdRaw();
        var userId = GetCurrentUserId();
        if (userId is null || userIdRaw is null)
            return Unauthorized();

        var canAccess = await _lessonAccess.CanStudentAccessBlockAsync(id, userIdRaw, cancellationToken);
        if (!canAccess)
            return Forbid();

        var safeTake = Math.Clamp(take, 1, 50);
        var runs = await _contentDb.CodeExerciseRuns
            .AsNoTracking()
            .Include(r => r.Attempt)
            .Where(r => r.BlockId == id && r.UserId == userId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .Take(safeTake)
            .ToListAsync(cancellationToken);

        var result = runs
            .Select(MapRunDto)
            .Select(SanitizeRunForStudent)
            .ToList();

        return Ok(result);
    }

    [HttpPost("{id:guid}/execute-code")]
    [Authorize]
    [ProducesResponseType(typeof(CodeExecutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecuteCode(Guid id, [FromBody] ExecuteCodeRequest request, CancellationToken cancellationToken)
    {
        var userIdRaw = GetCurrentUserIdRaw();
        var userId = GetCurrentUserId();
        if (userIdRaw is null || userId is null)
            return Unauthorized();

        var includeSensitiveData = IsAdmin();
        if (!includeSensitiveData && IsTeacher())
        {
            var canManage = await _lessonAccess.CanTeacherManageBlockAsync(id, userIdRaw, cancellationToken);
            if (!canManage)
                return Forbid();

            includeSensitiveData = true;
        }
        else if (!includeSensitiveData && IsStudent())
        {
            var canAccess = await _lessonAccess.CanStudentAccessBlockAsync(id, userIdRaw, cancellationToken);
            if (!canAccess)
                return Forbid();
        }
        else if (!includeSensitiveData)
        {
            return Forbid();
        }

        var result = await _mediator.Send(new ExecuteCodeCommand(id, userId.Value, request.Code), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "CODE_EXECUTE_FAILED"));

        var response = includeSensitiveData
            ? result.Value
            : CodeExerciseSanitizer.SanitizeExecutionResponseForStudent(result.Value!);

        return Ok(response);
    }

    private async Task<CodeExerciseBlockData?> LoadCodeExerciseDataAsync(Guid blockId, CancellationToken cancellationToken)
    {
        var blockData = await _contentDb.LessonBlocks
            .AsNoTracking()
            .Where(b => b.Id == blockId)
            .Select(b => b.Data)
            .FirstOrDefaultAsync(cancellationToken);

        return blockData as CodeExerciseBlockData;
    }

    private static LessonBlockDto SanitizeBlockForStudent(LessonBlockDto block)
    {
        if (block.Data is not CodeExerciseBlockData codeData)
            return block;

        return new LessonBlockDto
        {
            Id = block.Id,
            LessonId = block.LessonId,
            OrderIndex = block.OrderIndex,
            Type = block.Type,
            Data = CodeExerciseSanitizer.SanitizeBlockDataForStudent(codeData),
            Settings = block.Settings,
            CreatedAt = block.CreatedAt,
            UpdatedAt = block.UpdatedAt
        };
    }

    private static LessonBlockAttemptDto SanitizeAttemptForStudent(LessonBlockAttemptDto attempt, CodeExerciseBlockData? codeData)
    {
        if (attempt.Answers is not CodeExerciseAnswer codeAnswer)
            return attempt;

        return new LessonBlockAttemptDto
        {
            Id = attempt.Id,
            BlockId = attempt.BlockId,
            UserId = attempt.UserId,
            Answers = CodeExerciseSanitizer.SanitizeAnswerForStudent(codeAnswer, codeData),
            Score = attempt.Score,
            MaxScore = attempt.MaxScore,
            IsCorrect = attempt.IsCorrect,
            NeedsReview = attempt.NeedsReview,
            AttemptsUsed = attempt.AttemptsUsed,
            Status = attempt.Status,
            SubmittedAt = attempt.SubmittedAt,
            ReviewedAt = attempt.ReviewedAt,
            ReviewerId = attempt.ReviewerId,
            ReviewerComment = attempt.ReviewerComment
        };
    }

    private static CodeExerciseRunDto MapRunDto(CodeExerciseRun run)
    {
        return new CodeExerciseRunDto
        {
            Id = run.Id,
            BlockId = run.BlockId,
            UserId = run.UserId,
            AttemptId = run.AttemptId,
            Kind = run.Kind.ToString(),
            Language = run.Language,
            Code = run.Code,
            Ok = run.Ok,
            GlobalError = run.GlobalError,
            Results = CodeExerciseSanitizer.CloneRunResults(run.Results),
            CreatedAt = run.CreatedAt,
            AttemptStatus = run.Attempt?.Status.ToString(),
            AttemptScore = run.Attempt?.Score,
            AttemptMaxScore = run.Attempt?.MaxScore,
            AttemptNeedsReview = run.Attempt?.NeedsReview,
            AttemptReviewedAt = run.Attempt?.ReviewedAt,
            AttemptReviewerComment = run.Attempt?.ReviewerComment,
            AttemptAttemptsUsed = run.Attempt?.AttemptsUsed
        };
    }

    private static CodeExerciseRunDto SanitizeRunForStudent(CodeExerciseRunDto run)
    {
        return new CodeExerciseRunDto
        {
            Id = run.Id,
            BlockId = run.BlockId,
            UserId = run.UserId,
            UserName = run.UserName,
            AttemptId = run.AttemptId,
            Kind = run.Kind,
            Language = run.Language,
            Code = run.Code,
            Ok = run.Ok,
            GlobalError = run.GlobalError,
            Results = CodeExerciseSanitizer.SanitizeRunResultsForStudent(run.Results),
            CreatedAt = run.CreatedAt,
            BlockOrderIndex = run.BlockOrderIndex,
            BlockLabel = run.BlockLabel,
            AttemptStatus = run.AttemptStatus,
            AttemptScore = run.AttemptScore,
            AttemptMaxScore = run.AttemptMaxScore,
            AttemptNeedsReview = run.AttemptNeedsReview,
            AttemptReviewedAt = run.AttemptReviewedAt,
            AttemptReviewerComment = run.AttemptReviewerComment,
            AttemptAttemptsUsed = run.AttemptAttemptsUsed
        };
    }

    private string? GetCurrentUserIdRaw()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
    }

    private Guid? GetCurrentUserId()
    {
        var id = GetCurrentUserIdRaw();
        return Guid.TryParse(id, out var g) ? g : null;
    }

    private bool IsTeacher() => User.IsInRole("Teacher");
    private bool IsStudent() => User.IsInRole("Student");
    private bool IsAdmin() => User.IsInRole("Admin");
}

public record UpdateLessonBlockRequest(
    Content.Domain.ValueObjects.Blocks.LessonBlockData Data,
    Content.Domain.ValueObjects.Blocks.LessonBlockSettings? Settings);

public record SubmitAttemptRequest(LessonBlockAnswer Answers);

public record ExecuteCodeRequest(string Code);
