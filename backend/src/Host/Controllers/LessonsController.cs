using Courses.Application.DTOs;
using Courses.Application.Lessons.Commands.CreateLesson;
using Courses.Application.Lessons.Commands.DeleteLesson;
using Courses.Application.Lessons.Commands.ReorderLessons;
using Courses.Application.Lessons.Commands.UpdateLesson;
using Courses.Application.Lessons.Queries.GetLessonById;
using Courses.Application.Lessons.Queries.GetModuleLessons;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Host.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/lessons")]
public class LessonsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly LessonAccessService _lessonAccess;
    private readonly CourseItemSyncService _courseItems;

    public LessonsController(
        IMediator mediator,
        LessonAccessService lessonAccess,
        CourseItemSyncService courseItems)
    {
        _mediator = mediator;
        _lessonAccess = lessonAccess;
        _courseItems = courseItems;
    }

    [HttpGet("by-module/{moduleId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(List<LessonDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByModule(Guid moduleId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        if (!await CanViewModuleAsync(moduleId, userId, cancellationToken))
            return Forbid();

        var result = await _mediator.Send(new GetModuleLessonsQuery(moduleId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        if (!await CanViewLessonAsync(id, userId, cancellationToken))
            return Forbid();

        var result = await _mediator.Send(new GetLessonByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLessonCommand command, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageModuleAsync(command.ModuleId, userId, cancellationToken);
        if (!canManage)
            return Forbid();

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "LESSON_CREATE_FAILED"));

        await _courseItems.EnsureLessonItemAsync(result.Value!.Id, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageLessonAsync(id, userId, cancellationToken);
        if (!canManage)
            return Forbid();

        var command = new UpdateLessonCommand(id, userId, request.Title, request.Description, request.Duration, request.IsPublished, request.Layout, request.ModuleId);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "LESSON_UPDATE_FAILED"));

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

        var canManage = await _lessonAccess.CanTeacherManageLessonAsync(id, userId, cancellationToken);
        if (!canManage)
            return Forbid();

        var result = await _mediator.Send(new DeleteLessonCommand(id), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "LESSON_NOT_FOUND"));

        await _courseItems.DeleteBySourceAsync(CourseItemType.Lesson, id, cancellationToken);
        return Ok(new { message = result.Value });
    }

    [HttpPost("reorder")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reorder([FromBody] ReorderLessonsCommand command, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserIdRaw();
        if (userId is null)
            return Unauthorized();

        var canManage = await _lessonAccess.CanTeacherManageModuleAsync(command.ModuleId, userId, cancellationToken);
        if (!canManage)
            return Forbid();

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "LESSON_REORDER_FAILED"));

        return Ok(new { message = result.Value });
    }

    private async Task<bool> CanViewModuleAsync(Guid moduleId, string userId, CancellationToken cancellationToken)
    {
        if (IsAdmin())
            return true;

        if (IsTeacher())
            return await _lessonAccess.CanTeacherManageModuleAsync(moduleId, userId, cancellationToken);

        if (IsStudent())
            return await _lessonAccess.CanStudentAccessModuleAsync(moduleId, userId, cancellationToken);

        return false;
    }

    private async Task<bool> CanViewLessonAsync(Guid lessonId, string userId, CancellationToken cancellationToken)
    {
        if (IsAdmin())
            return true;

        if (IsTeacher())
            return await _lessonAccess.CanTeacherManageLessonAsync(lessonId, userId, cancellationToken);

        if (IsStudent())
            return await _lessonAccess.CanStudentAccessLessonAsync(lessonId, userId, cancellationToken);

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

public record UpdateLessonRequest(string Title, string? Description, int? Duration, bool? IsPublished, LessonLayout? Layout = null, Guid? ModuleId = null);
