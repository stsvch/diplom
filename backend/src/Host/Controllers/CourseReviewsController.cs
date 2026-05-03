using EduPlatform.Host.Models.Courses;
using EduPlatform.Host.Services;
using EduPlatform.Shared.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/courses/{courseId:guid}/reviews")]
public class CourseReviewsController : ControllerBase
{
    private readonly CourseReviewService _reviews;

    public CourseReviewsController(CourseReviewService reviews)
    {
        _reviews = reviews;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CourseReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviews(Guid courseId, CancellationToken cancellationToken)
    {
        return Ok(await _reviews.GetReviewsAsync(courseId, cancellationToken));
    }

    [HttpPut("mine")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(CourseReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertMine(
        Guid courseId,
        [FromBody] UpsertCourseReviewRequest request,
        CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized();

        var studentName = $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}".Trim();
        if (string.IsNullOrWhiteSpace(studentName))
            studentName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Студент";

        var result = await _reviews.UpsertReviewAsync(courseId, studentId, studentName, request, cancellationToken);
        if (!result.Success)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_REVIEW_FAILED"));

        return Ok(result.Review);
    }

    [HttpDelete("{reviewId:guid}")]
    [Authorize(Roles = "Student,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(
        Guid courseId,
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _reviews.DeleteReviewAsync(courseId, reviewId, userId, User.IsInRole("Admin"), cancellationToken);
        if (!result.Success)
            return BadRequest(ApiError.FromMessage(result.Error!, "COURSE_REVIEW_DELETE_FAILED"));

        return Ok(new { message = "Отзыв удалён." });
    }
}
