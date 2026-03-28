using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tests.Application.Attempts.Commands.SaveAnswer;
using Tests.Application.Attempts.Commands.StartAttempt;
using Tests.Application.Attempts.Commands.SubmitAttempt;
using Tests.Application.Attempts.Queries.GetAttempt;
using Tests.Application.Attempts.Queries.GetMyAttempts;
using Tests.Application.DTOs;
using Tests.Application.Tests.Commands.GradeResponse;

namespace EduPlatform.Host.Controllers;

[ApiController]
public class AttemptsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttemptsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("api/tests/{testId:guid}/start")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(TestAttemptStartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartAttempt(Guid testId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new StartAttemptCommand(testId, userId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "ATTEMPT_START_FAILED"));

        return Ok(result.Value);
    }

    [HttpPost("api/attempts/{id:guid}/answer")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveAnswer(Guid id, [FromBody] SaveAnswerRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new SaveAnswerCommand(id, userId, request.QuestionId, request.SelectedOptionIds, request.TextAnswer);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "ANSWER_SAVE_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("api/attempts/{id:guid}/submit")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(TestAttemptDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitAttempt(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new SubmitAttemptCommand(id, userId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "ATTEMPT_SUBMIT_FAILED"));

        return Ok(result.Value);
    }

    [HttpGet("api/attempts/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TestAttemptDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttempt(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAttemptQuery(id), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "ATTEMPT_NOT_FOUND"));

        var attempt = result.Value!;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        // Студент может видеть только свои попытки
        if (role == "Student" && attempt.StudentId != userId)
            return NotFound(ApiError.FromMessage("Попытка не найдена.", "ATTEMPT_NOT_FOUND"));

        return Ok(attempt);
    }

    [HttpGet("api/tests/{testId:guid}/my-attempts")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(List<TestAttemptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyAttempts(Guid testId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetMyAttemptsQuery(testId, userId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "ATTEMPTS_GET_FAILED"));

        return Ok(result.Value);
    }

    [HttpPut("api/responses/{id:guid}/grade")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GradeResponse(Guid id, [FromBody] GradeResponseRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new GradeResponseCommand(id, userId, request.Points, request.Comment);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "GRADE_FAILED"));

        return Ok(new { message = result.Value });
    }
}

public record SaveAnswerRequest(Guid QuestionId, List<string>? SelectedOptionIds, string? TextAnswer);

public record GradeResponseRequest(int Points, string? Comment);
