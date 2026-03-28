using Courses.Application.DTOs;
using Courses.Application.LessonBlocks.Commands.CreateLessonBlock;
using Courses.Application.LessonBlocks.Commands.DeleteLessonBlock;
using Courses.Application.LessonBlocks.Commands.ReorderBlocks;
using Courses.Application.LessonBlocks.Commands.UpdateLessonBlock;
using Courses.Application.LessonBlocks.Queries.GetLessonBlocks;
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
        var command = new UpdateLessonBlockCommand(id, request.TextContent, request.VideoUrl, request.TestId, request.AssignmentId);
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
}

public record UpdateLessonBlockRequest(string? TextContent, string? VideoUrl, Guid? TestId, Guid? AssignmentId);
