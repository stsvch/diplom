using EduPlatform.Host.Models.Courses;
using EduPlatform.Host.Services;
using EduPlatform.Shared.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/courses/{courseId:guid}/builder")]
[Authorize(Roles = "Teacher,Admin")]
public class CourseBuilderController : ControllerBase
{
    private readonly CourseItemManagementService _courseItems;

    public CourseBuilderController(CourseItemManagementService courseItems)
    {
        _courseItems = courseItems;
    }

    [HttpPost("backfill")]
    [ProducesResponseType(typeof(CourseItemBackfillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Backfill(Guid courseId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _courseItems.BackfillAsync(
            courseId,
            userId,
            User.IsInRole("Admin"),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(CourseItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateStandaloneItem(
        Guid courseId,
        [FromBody] CreateStandaloneCourseItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _courseItems.CreateStandaloneAsync(
            courseId,
            request,
            userId,
            User.IsInRole("Admin"),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPost("items/{itemId:guid}/move")]
    [ProducesResponseType(typeof(CourseItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MoveItem(
        Guid courseId,
        Guid itemId,
        [FromBody] MoveCourseItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _courseItems.MoveAsync(
            courseId,
            itemId,
            request,
            userId,
            User.IsInRole("Admin"),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPost("items/reorder")]
    [ProducesResponseType(typeof(List<CourseItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReorderItems(
        Guid courseId,
        [FromBody] ReorderCourseItemsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _courseItems.ReorderAsync(
            courseId,
            request,
            userId,
            User.IsInRole("Admin"),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("items/{itemId:guid}")]
    [ProducesResponseType(typeof(CourseItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStandaloneItem(
        Guid courseId,
        Guid itemId,
        [FromBody] UpdateStandaloneCourseItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _courseItems.UpdateStandaloneAsync(
            courseId,
            itemId,
            request,
            userId,
            User.IsInRole("Admin"),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("items/{itemId:guid}/metadata")]
    [ProducesResponseType(typeof(CourseItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateItemMetadata(
        Guid courseId,
        Guid itemId,
        [FromBody] UpdateCourseItemMetadataRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _courseItems.UpdateMetadataAsync(
            courseId,
            itemId,
            request,
            userId,
            User.IsInRole("Admin"),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpDelete("items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteStandaloneItem(
        Guid courseId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _courseItems.DeleteStandaloneAsync(
            courseId,
            itemId,
            userId,
            User.IsInRole("Admin"),
            cancellationToken);

        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(CourseItemMutationResult<T> result)
    {
        return result.Status switch
        {
            CourseItemMutationStatus.Success => Ok(result.Value),
            CourseItemMutationStatus.Forbidden => Forbid(),
            CourseItemMutationStatus.ValidationFailed => BadRequest(
                ApiError.FromMessage(result.Error ?? "Некорректные данные.", "COURSE_ITEM_VALIDATION_FAILED")),
            _ => NotFound(ApiError.FromMessage(result.Error ?? "Элемент курса не найден.", "COURSE_ITEM_NOT_FOUND"))
        };
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirst("sub")?.Value;
    }
}
