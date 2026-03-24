using Courses.Application.DTOs;
using Courses.Application.Modules.Commands.CreateModule;
using Courses.Application.Modules.Commands.DeleteModule;
using Courses.Application.Modules.Commands.ReorderModules;
using Courses.Application.Modules.Commands.UpdateModule;
using Courses.Application.Modules.Queries.GetCourseModules;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/modules")]
public class ModulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("by-course/{courseId:guid}")]
    [ProducesResponseType(typeof(List<CourseModuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCourseModulesQuery(courseId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(CourseModuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateModuleCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "MODULE_CREATE_FAILED"));

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(CourseModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateModuleRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateModuleCommand(id, request.Title, request.Description, request.IsPublished);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "MODULE_NOT_FOUND"));

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteModuleCommand(id), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "MODULE_NOT_FOUND"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("reorder")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reorder([FromBody] ReorderModulesCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "MODULE_REORDER_FAILED"));

        return Ok(new { message = result.Value });
    }
}

public record UpdateModuleRequest(string Title, string? Description, bool? IsPublished);
