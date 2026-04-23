using Courses.Application.Courses.Commands.ForceArchiveCourse;
using Courses.Application.Courses.Queries.GetAllCoursesAdmin;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/admin/courses")]
[Authorize(Roles = "Admin")]
public class AdminCoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCoursesController(IMediator mediator)
    {
        _mediator = mediator;
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
}

public record ForceArchiveRequest(string Reason);
