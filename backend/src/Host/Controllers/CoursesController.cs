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
using Auth.Domain.Entities;
using EduPlatform.Host.Models.Courses;
using EduPlatform.Host.Services;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CourseBuilderReadService _courseBuilder;

    public CoursesController(
        IMediator mediator,
        UserManager<ApplicationUser> userManager,
        CourseBuilderReadService courseBuilder)
    {
        _mediator = mediator;
        _userManager = userManager;
        _courseBuilder = courseBuilder;
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
        await EnrichTeacherNamesAsync(result.Items, cancellationToken);
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

        await EnrichTeacherNamesAsync(result.Value, cancellationToken);
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
        await EnrichTeacherNamesAsync(result, cancellationToken);
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

        var given = User.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
        var surname = User.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;
        var userName = $"{given} {surname}".Trim();
        if (string.IsNullOrWhiteSpace(userName))
            userName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Преподаватель";
        var command = new CreateCourseCommand(
            userId, userName, request.DisciplineId, request.Title, request.Description,
            request.Price, request.IsFree, request.OrderType, request.HasGrading,
            request.Level, request.ImageUrl, request.Tags,
            request.HasCertificate, request.Deadline);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_CREATE_FAILED"));

        await EnrichTeacherNamesAsync(result.Value, cancellationToken);
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

        await EnrichTeacherNamesAsync(result.Value, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/builder")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(typeof(CourseBuilderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBuilder(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _courseBuilder.GetAsync(id, userId, User.IsInRole("Admin"), cancellationToken);
        return result.Status switch
        {
            CourseBuilderReadStatus.Success => Ok(result.Builder),
            CourseBuilderReadStatus.Forbidden => Forbid(),
            _ => NotFound(ApiError.FromMessage("Курс не найден.", "COURSE_NOT_FOUND"))
        };
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

        var builderResult = await _courseBuilder.GetAsync(id, userId, User.IsInRole("Admin"), cancellationToken);
        if (builderResult.Status == CourseBuilderReadStatus.NotFound)
            return BadRequest(ApiError.FromMessage("Курс не найден.", "COURSE_PUBLISH_FAILED"));
        if (builderResult.Status == CourseBuilderReadStatus.Forbidden)
            return Forbid();

        var readinessIssues = builderResult.Builder!.Readiness.Issues
            .Select(ToPublishIssue)
            .ToList();

        if (readinessIssues.Any(i => i.Type == "error") && !force)
        {
            return Ok(new PublishValidationResult
            {
                Success = false,
                Message = "Курс не может быть опубликован — есть ошибки в готовности курса.",
                Issues = readinessIssues
            });
        }

        var result = await _mediator.Send(new PublishCourseCommand(id, userId, force), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_PUBLISH_FAILED"));

        foreach (var issue in readinessIssues)
        {
            if (!result.Value!.Issues.Any(i => i.Code == issue.Code && i.Path == issue.Path))
                result.Value.Issues.Add(issue);
        }

        if (result.Value!.Success && readinessIssues.Any(i => i.Type == "error"))
            result.Value.Message = "Курс опубликован принудительно, но в готовности курса остаются ошибки.";

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

    private async Task EnrichTeacherNamesAsync(IEnumerable<CourseListDto> courses, CancellationToken cancellationToken)
    {
        var items = courses
            .Where(c =>
                !string.IsNullOrWhiteSpace(c.TeacherId)
                && (string.IsNullOrWhiteSpace(c.TeacherName) || c.TeacherName == "Unknown"))
            .ToList();

        if (items.Count == 0)
            return;

        var teacherIds = items
            .Select(c => c.TeacherId)
            .Distinct()
            .ToList();

        var users = await _userManager.Users
            .Where(u => teacherIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        foreach (var item in items)
        {
            if (!users.TryGetValue(item.TeacherId, out var user))
                continue;

            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            item.TeacherName = string.IsNullOrWhiteSpace(fullName)
                ? user.Email ?? "Преподаватель"
                : fullName;
        }
    }

    private Task EnrichTeacherNamesAsync(CourseDetailDto? course, CancellationToken cancellationToken)
    {
        if (course is null)
            return Task.CompletedTask;

        return EnrichTeacherNamesAsync(new[] { course }, cancellationToken);
    }

    private static PublishIssue ToPublishIssue(CourseBuilderReadinessIssueDto issue)
    {
        var type = issue.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)
            ? "error"
            : "warning";

        var path = issue.SourceId.HasValue
            ? $"{issue.ItemType ?? "item"}-{issue.SourceId.Value}"
            : issue.SectionId.HasValue
                ? $"section-{issue.SectionId.Value}"
                : "course";

        return new PublishIssue(type, path, issue.Code, issue.Message);
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
