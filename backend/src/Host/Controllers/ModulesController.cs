using Courses.Application.DTOs;
using Courses.Application.Modules.Commands.CreateModule;
using Courses.Application.Modules.Commands.DeleteModule;
using Courses.Application.Modules.Commands.ReorderModules;
using Courses.Application.Modules.Commands.UpdateModule;
using Courses.Application.Modules.Queries.GetCourseModules;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Host.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/modules")]
public class ModulesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly LessonAccessService _lessonAccess;

    public ModulesController(IMediator mediator, LessonAccessService lessonAccess)
    {
        _mediator = mediator;
        _lessonAccess = lessonAccess;
    }

    [HttpGet("by-course/{courseId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(List<CourseModuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        if (!await CanViewCourseAsync(courseId, userId, cancellationToken))
            return Forbid();

        var result = await _mediator.Send(new GetCourseModulesQuery(courseId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(CourseModuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateModuleCommand command, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageCourseAsync(command.CourseId, userId, cancellationToken);
        if (!canManage)
            return Forbid();

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
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageModuleAsync(id, userId, cancellationToken);
        if (!canManage)
            return Forbid();

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
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageModuleAsync(id, userId, cancellationToken);
        if (!canManage)
            return Forbid();

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
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageCourseAsync(command.CourseId, userId, cancellationToken);
        if (!canManage)
            return Forbid();

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "MODULE_REORDER_FAILED"));

        return Ok(new { message = result.Value });
    }

    private async Task<bool> CanViewCourseAsync(Guid courseId, string userId, CancellationToken cancellationToken)
    {
        if (IsAdmin())
            return true;

        if (IsTeacher())
            return await _lessonAccess.CanTeacherManageCourseAsync(courseId, userId, cancellationToken);

        if (IsStudent())
            return await _lessonAccess.CanStudentAccessCourseAsync(courseId, userId, cancellationToken);

        return false;
    }

    private string? GetCurrentUserIdRaw()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirst("sub")?.Value;
    }

    private bool IsTeacher() => User.IsInRole("Teacher");
    private bool IsStudent() => User.IsInRole("Student");
    private bool IsAdmin() => User.IsInRole("Admin");
}

public record UpdateModuleRequest(string Title, string? Description, bool? IsPublished);
