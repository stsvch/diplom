using Content.Application.Attempts.Commands.ReviewAttempt;
using Content.Application.Attempts.Queries.GetLessonAttempts;
using Content.Application.Attempts.Queries.GetLessonProgress;
using Content.Application.DTOs;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api")]
public class LessonProgressController : ControllerBase
{
    private readonly IMediator _mediator;

    public LessonProgressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("lessons/{lessonId:guid}/my-progress")]
    [Authorize]
    [ProducesResponseType(typeof(LessonProgressDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProgress(Guid lessonId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(new GetLessonProgressQuery(lessonId, userId.Value), cancellationToken);
        return Ok(result);
    }

    [HttpGet("lessons/{lessonId:guid}/attempts")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(List<LessonBlockAttemptDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessonAttempts(Guid lessonId, [FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLessonAttemptsQuery(lessonId, userId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("lesson-block-attempts/{id:guid}/review")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(LessonBlockAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewAttemptRequest request, CancellationToken cancellationToken)
    {
        var reviewerId = GetCurrentUserId();
        if (reviewerId is null) return Unauthorized();

        var command = new ReviewAttemptCommand(id, reviewerId.Value, request.Score, request.Comment);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "ATTEMPT_REVIEW_FAILED"));

        return Ok(result.Value);
    }

    private Guid? GetCurrentUserId()
    {
        var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
              ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(id, out var g) ? g : null;
    }
}

public record ReviewAttemptRequest(decimal Score, string? Comment);
