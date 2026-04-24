using Auth.Domain.Entities;
using Content.Application.Attempts.Commands.ReviewAttempt;
using Content.Application.Attempts.Queries.GetLessonAttempts;
using Content.Application.Attempts.Queries.GetLessonProgress;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using Content.Application.CodeExecution;
using Content.Domain.Entities;
using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Host.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api")]
public class LessonProgressController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly LessonAccessService _lessonAccess;
    private readonly IContentDbContext _contentDb;
    private readonly UserManager<ApplicationUser> _userManager;

    public LessonProgressController(
        IMediator mediator,
        LessonAccessService lessonAccess,
        IContentDbContext contentDb,
        UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _lessonAccess = lessonAccess;
        _contentDb = contentDb;
        _userManager = userManager;
    }

    [HttpGet("lessons/{lessonId:guid}/my-progress")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(LessonProgressDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProgress(Guid lessonId, CancellationToken cancellationToken)
    {
        var userIdRaw = GetCurrentUserIdRaw();
        var userId = GetCurrentUserId();
        if (userId is null || userIdRaw is null) return Unauthorized();

        var canAccess = await _lessonAccess.CanStudentAccessLessonAsync(lessonId, userIdRaw, cancellationToken);
        if (!canAccess) return Forbid();

        var result = await _mediator.Send(new GetLessonProgressQuery(lessonId, userId.Value), cancellationToken);
        return Ok(result);
    }

    [HttpGet("lessons/{lessonId:guid}/attempts")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(List<LessonBlockAttemptDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessonAttempts(Guid lessonId, [FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        var teacherId = GetCurrentUserIdRaw();
        if (teacherId is null) return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageLessonAsync(lessonId, teacherId, cancellationToken);
        if (!canManage) return Forbid();

        var result = await _mediator.Send(new GetLessonAttemptsQuery(lessonId, userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("lessons/{lessonId:guid}/code-runs")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(List<CodeExerciseRunDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessonCodeRuns(
        Guid lessonId,
        [FromQuery] Guid? blockId,
        [FromQuery] Guid? userId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var teacherId = GetCurrentUserIdRaw();
        if (teacherId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageLessonAsync(lessonId, teacherId, cancellationToken);
        if (!canManage)
            return Forbid();

        var codeBlocks = await _contentDb.LessonBlocks
            .AsNoTracking()
            .Where(b => b.LessonId == lessonId && b.Type == LessonBlockType.CodeExercise)
            .OrderBy(b => b.OrderIndex)
            .ToListAsync(cancellationToken);

        if (codeBlocks.Count == 0)
            return Ok(new List<CodeExerciseRunDto>());

        var blockInfo = codeBlocks.ToDictionary(
            b => b.Id,
            b => new
            {
                b.OrderIndex,
                Label = BuildBlockLabel(b)
            });

        if (blockId.HasValue && !blockInfo.ContainsKey(blockId.Value))
            return Ok(new List<CodeExerciseRunDto>());

        var safeTake = Math.Clamp(take, 1, 200);
        var blockIds = blockId.HasValue
            ? new List<Guid> { blockId.Value }
            : blockInfo.Keys.ToList();

        var query = _contentDb.CodeExerciseRuns
            .AsNoTracking()
            .Include(r => r.Attempt)
            .Where(r => blockIds.Contains(r.BlockId));

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        var runs = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(safeTake)
            .ToListAsync(cancellationToken);

        var userNames = await LoadUserNamesAsync(runs.Select(r => r.UserId.ToString()), cancellationToken);

        var result = runs
            .Select(run =>
            {
                var info = blockInfo[run.BlockId];
                return MapRunDto(
                    run,
                    userNames.TryGetValue(run.UserId.ToString(), out var userName) ? userName : run.UserId.ToString(),
                    info.OrderIndex,
                    info.Label);
            })
            .ToList();

        return Ok(result);
    }

    [HttpPost("lesson-block-attempts/{id:guid}/review")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(LessonBlockAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewAttemptRequest request, CancellationToken cancellationToken)
    {
        var reviewerIdRaw = GetCurrentUserIdRaw();
        var reviewerId = GetCurrentUserId();
        if (reviewerId is null || reviewerIdRaw is null) return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageAttemptAsync(id, reviewerIdRaw, cancellationToken);
        if (!canManage) return Forbid();

        var command = new ReviewAttemptCommand(id, reviewerId.Value, request.Score, request.Comment);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "ATTEMPT_REVIEW_FAILED"));

        return Ok(result.Value);
    }

    private Guid? GetCurrentUserId()
    {
        var id = GetCurrentUserIdRaw();
        return Guid.TryParse(id, out var g) ? g : null;
    }

    private string? GetCurrentUserIdRaw()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
    }

    private async Task<Dictionary<string, string>> LoadUserNamesAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        var ids = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return new Dictionary<string, string>();

        return await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.UserName })
            .ToDictionaryAsync(
                u => u.Id,
                u =>
                {
                    var fullName = $"{u.FirstName} {u.LastName}".Trim();
                    if (!string.IsNullOrWhiteSpace(fullName))
                        return fullName;

                    return u.Email ?? u.UserName ?? u.Id;
                },
                ct);
    }

    private static CodeExerciseRunDto MapRunDto(CodeExerciseRun run, string userName, int blockOrderIndex, string blockLabel)
    {
        return new CodeExerciseRunDto
        {
            Id = run.Id,
            BlockId = run.BlockId,
            UserId = run.UserId,
            UserName = userName,
            AttemptId = run.AttemptId,
            Kind = run.Kind.ToString(),
            Language = run.Language,
            Code = run.Code,
            Ok = run.Ok,
            GlobalError = run.GlobalError,
            Results = CodeExerciseSanitizer.CloneRunResults(run.Results),
            CreatedAt = run.CreatedAt,
            BlockOrderIndex = blockOrderIndex,
            BlockLabel = blockLabel,
            AttemptStatus = run.Attempt?.Status.ToString(),
            AttemptScore = run.Attempt?.Score,
            AttemptMaxScore = run.Attempt?.MaxScore,
            AttemptNeedsReview = run.Attempt?.NeedsReview,
            AttemptReviewedAt = run.Attempt?.ReviewedAt,
            AttemptReviewerComment = run.Attempt?.ReviewerComment,
            AttemptAttemptsUsed = run.Attempt?.AttemptsUsed
        };
    }

    private static string BuildBlockLabel(LessonBlock block)
    {
        if (block.Data is CodeExerciseBlockData codeData)
        {
            var instruction = codeData.Instruction?.Trim();
            if (!string.IsNullOrWhiteSpace(instruction))
            {
                return instruction.Length > 96 ? instruction[..96] + "…" : instruction;
            }
        }

        return $"CodeExercise #{block.OrderIndex + 1}";
    }
}

public record ReviewAttemptRequest(decimal Score, string? Comment);
