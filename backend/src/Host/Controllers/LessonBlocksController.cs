using Content.Application.Attempts.Commands.SubmitAttempt;
using Content.Application.Attempts.Queries.GetMyAttempt;
using Content.Application.CodeExecution;
using Content.Application.DTOs;
using Content.Application.LessonBlocks.Commands.CreateLessonBlock;
using Content.Application.LessonBlocks.Commands.DeleteLessonBlock;
using Content.Application.LessonBlocks.Commands.ReorderBlocks;
using Content.Application.LessonBlocks.Commands.UpdateLessonBlock;
using Content.Application.LessonBlocks.Queries.GetLessonBlocks;
using Content.Domain.ValueObjects.Answers;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/lesson-blocks")]
public class LessonBlocksController : ControllerBase
{
    private readonly IMediator _mediator;

    public LessonBlocksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("by-lesson/{lessonId:guid}")]
    [ProducesResponseType(typeof(List<LessonBlockDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLessonBlocksQuery(lessonId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(LessonBlockDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLessonBlockCommand command, CancellationToken cancellationToken)
    {
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
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "BLOCK_REORDER_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("{id:guid}/attempts")]
    [Authorize]
    [ProducesResponseType(typeof(SubmitAttemptResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitAttempt(Guid id, [FromBody] SubmitAttemptRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var command = new SubmitAttemptCommand(id, userId.Value, request.Answers);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "ATTEMPT_SUBMIT_FAILED"));

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/my-attempt")]
    [Authorize]
    [ProducesResponseType(typeof(LessonBlockAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetMyAttempt(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _mediator.Send(new GetMyAttemptQuery(id, userId.Value), cancellationToken);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPost("{id:guid}/execute-code")]
    [Authorize]
    [ProducesResponseType(typeof(CodeExecutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecuteCode(Guid id, [FromBody] ExecuteCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ExecuteCodeCommand(id, request.Code), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "CODE_EXECUTE_FAILED"));
        return Ok(result.Value);
    }

    private Guid? GetCurrentUserId()
    {
        var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
              ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(id, out var g) ? g : null;
    }
}

public record UpdateLessonBlockRequest(
    Content.Domain.ValueObjects.Blocks.LessonBlockData Data,
    Content.Domain.ValueObjects.Blocks.LessonBlockSettings? Settings);

public record SubmitAttemptRequest(LessonBlockAnswer Answers);

public record ExecuteCodeRequest(string Code);
