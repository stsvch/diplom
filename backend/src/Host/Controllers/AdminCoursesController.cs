using Courses.Application.Courses.Commands.ForceArchiveCourse;
using Courses.Application.Courses.Queries.GetAllCoursesAdmin;
using Auth.Domain.Entities;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/admin/courses")]
[Authorize(Roles = "Admin")]
public class AdminCoursesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminCoursesController(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminCourseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] Guid? disciplineId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllCoursesAdminQuery(search, status, disciplineId, page, pageSize), cancellationToken);
        if (result.IsSuccess && result.Value is not null)
            await EnrichTeacherNamesAsync(result.Value.Items, cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "COURSES_LIST_FAILED"))
            : Ok(result.Value);
    }

    [HttpPost("{id:guid}/force-archive")]
    public async Task<IActionResult> ForceArchive(Guid id, [FromBody] ForceArchiveRequest request, CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _mediator.Send(new ForceArchiveCourseCommand(id, adminId, request.Reason), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "COURSE_FORCE_ARCHIVE_FAILED"))
            : Ok(new { message = result.Value });
    }

    private async Task EnrichTeacherNamesAsync(IEnumerable<AdminCourseDto> courses, CancellationToken cancellationToken)
    {
        var items = courses
            .Where(c => !string.IsNullOrWhiteSpace(c.TeacherId) && (string.IsNullOrWhiteSpace(c.TeacherName) || c.TeacherName == "Unknown"))
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
            if (users.TryGetValue(item.TeacherId, out var user))
            {
                var fullName = $"{user.FirstName} {user.LastName}".Trim();
                item.TeacherName = string.IsNullOrWhiteSpace(fullName)
                    ? user.Email ?? "Преподаватель"
                    : fullName;
            }
        }
    }
}

public record ForceArchiveRequest(string Reason);
