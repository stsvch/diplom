using Courses.Application.Courses.Commands.ArchiveCourse;
using Courses.Application.Courses.Commands.CreateCourse;
using Courses.Application.Courses.Commands.DeleteCourse;
using Courses.Application.Courses.Commands.EnrollCourse;
using Courses.Application.Courses.Commands.PublishCourse;
using Courses.Application.Courses.Commands.UnenrollCourse;
using Courses.Application.Courses.Commands.UpdateCourse;
using Courses.Application.Courses.Queries.GetCourseCatalog;
using Courses.Application.Courses.Queries.GetCourseById;
using Courses.Application.Courses.Queries.GetMyCourses;
using Courses.Application.DTOs;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CourseListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog(
        [FromQuery] Guid? disciplineId,
        [FromQuery] bool? isFree,
        [FromQuery] CourseLevel? level,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCourseCatalogQuery(disciplineId, isFree, level, search, sortBy, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var result = await _mediator.Send(new GetCourseByIdQuery(id, userId, role), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "COURSE_NOT_FOUND"));

        return Ok(result.Value);
    }

    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(List<CourseListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCourses(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Student";
        var query = new GetMyCoursesQuery(userId, role);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
        var command = new CreateCourseCommand(
            userId, userName, request.DisciplineId, request.Title, request.Description,
            request.Price, request.IsFree, request.OrderType, request.HasGrading,
            request.Level, request.ImageUrl, request.Tags,
            request.HasCertificate, request.Deadline);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_CREATE_FAILED"));

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new UpdateCourseCommand(
            id, userId, request.DisciplineId, request.Title, request.Description,
            request.Price, request.IsFree, request.OrderType, request.HasGrading,
            request.Level, request.ImageUrl, request.Tags,
            request.HasCertificate, request.Deadline);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_UPDATE_FAILED"));

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(PublishValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Publish(Guid id, [FromQuery] bool force, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new PublishCourseCommand(id, userId, force), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_PUBLISH_FAILED"));

        return Ok(result.Value!);
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new ArchiveCourseCommand(id, userId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_ARCHIVE_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new DeleteCourseCommand(id, userId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_DELETE_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("{id:guid}/enroll")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Enroll(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var given = User.FindFirstValue(ClaimTypes.GivenName) ?? "";
        var surname = User.FindFirstValue(ClaimTypes.Surname) ?? "";
        var studentName = $"{given} {surname}".Trim();
        if (string.IsNullOrEmpty(studentName))
            studentName = User.FindFirstValue(ClaimTypes.Name) ?? "Студент";

        var result = await _mediator.Send(new EnrollCourseCommand(id, userId, studentName), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_ENROLL_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("{id:guid}/unenroll")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Unenroll(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new UnenrollCourseCommand(id, userId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_UNENROLL_FAILED"));

        return Ok(new { message = result.Value });
    }
}

public record CreateCourseRequest(
    Guid DisciplineId,
    string Title,
    string Description,
    decimal? Price,
    bool IsFree,
    CourseOrderType OrderType,
    bool HasGrading,
    CourseLevel Level,
    string? ImageUrl,
    string? Tags,
    bool HasCertificate = false,
    DateTime? Deadline = null);

public record UpdateCourseRequest(
    Guid DisciplineId,
    string Title,
    string Description,
    decimal? Price,
    bool IsFree,
    CourseOrderType OrderType,
    bool HasGrading,
    CourseLevel Level,
    string? ImageUrl,
    string? Tags,
    bool HasCertificate = false,
    DateTime? Deadline = null);
