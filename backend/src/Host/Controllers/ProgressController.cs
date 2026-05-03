using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Host.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Progress.Application.DTOs;
using Progress.Application.Interfaces;
using Progress.Application.Progress.Commands.CompleteLesson;
using Progress.Application.Progress.Commands.UncompleteLesson;
using Progress.Application.Progress.Queries.GetCourseProgress;
using Progress.Domain.Entities;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/progress")]
public class ProgressController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICoursesDbContext _coursesDb;
    private readonly IProgressDbContext _progressDb;
    private readonly LessonAccessService _lessonAccess;

    public ProgressController(
        IMediator mediator,
        ICoursesDbContext coursesDb,
        IProgressDbContext progressDb,
        LessonAccessService lessonAccess)
    {
        _mediator = mediator;
        _coursesDb = coursesDb;
        _progressDb = progressDb;
        _lessonAccess = lessonAccess;
    }

    [HttpPost("lessons/{lessonId:guid}/complete")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CompleteLesson(Guid lessonId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await _lessonAccess.CanStudentAccessLessonAsync(lessonId, studentId, ct))
            return Forbid();

        var result = await _mediator.Send(new CompleteLessonCommand(lessonId, studentId), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "COMPLETE_FAILED"));

        await SetCourseItemProgressBySourceAsync("Lesson", lessonId, studentId, true, ct);
        return Ok(result.Value);
    }

    [HttpDelete("lessons/{lessonId:guid}/complete")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UncompleteLesson(Guid lessonId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await _lessonAccess.CanStudentAccessLessonAsync(lessonId, studentId, ct))
            return Forbid();

        var result = await _mediator.Send(new UncompleteLessonCommand(lessonId, studentId), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "UNCOMPLETE_FAILED"));
        await SetCourseItemProgressBySourceAsync("Lesson", lessonId, studentId, false, ct);
        return Ok(new { message = "Lesson marked as not completed" });
    }

    [HttpPost("items/{courseItemId:guid}/complete")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CompleteCourseItem(Guid courseItemId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await SetCourseItemProgressAsync(courseItemId, studentId, true, ct);
        if (result.Forbidden) return Forbid();
        if (result.NotFound) return NotFound(ApiError.FromMessage("Элемент курса не найден.", "COURSE_ITEM_NOT_FOUND"));
        return Ok(result.Progress);
    }

    [HttpDelete("items/{courseItemId:guid}/complete")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UncompleteCourseItem(Guid courseItemId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await SetCourseItemProgressAsync(courseItemId, studentId, false, ct);
        if (result.Forbidden) return Forbid();
        if (result.NotFound) return NotFound(ApiError.FromMessage("Элемент курса не найден.", "COURSE_ITEM_NOT_FOUND"));
        return Ok(result.Progress);
    }

    [HttpGet("courses/{courseId:guid}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetCourseProgress(Guid courseId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var hasEnrollment = await _coursesDb.CourseEnrollments
            .AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId, ct);
        if (!hasEnrollment)
            return Forbid();

        var lessonIds = await _coursesDb.CourseModules
            .Where(m => m.CourseId == courseId)
            .SelectMany(m => m.Lessons.Select(l => l.Id))
            .ToListAsync(ct);

        var progress = await _mediator.Send(new GetCourseProgressQuery(courseId, studentId, lessonIds), ct);
        await FillItemProgressAsync(progress, courseId, studentId, ct);
        return Ok(progress);
    }

    [HttpGet("courses/{courseId:guid}/items")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetCourseItemProgress(Guid courseId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await CanStudentAccessCourseAsync(courseId, studentId, ct))
            return Forbid();

        var progress = new CourseProgressDto { CourseId = courseId };
        await FillItemProgressAsync(progress, courseId, studentId, ct);
        return Ok(progress);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyProgress(CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Get all lesson IDs this student has progress for
        var completedLessonIds = await _progressDb.LessonProgresses
            .Where(p => p.StudentId == studentId && p.IsCompleted)
            .Select(p => p.LessonId)
            .ToListAsync(ct);

        if (!completedLessonIds.Any())
            return Ok(new MyProgressDto { Courses = new List<CourseProgressDto>() });

        // Find which courses those lessons belong to
        var lessonCourseMap = await _coursesDb.CourseModules
            .Include(m => m.Lessons)
            .Where(m => m.Lessons.Any(l => completedLessonIds.Contains(l.Id)))
            .Select(m => new { m.CourseId, LessonIds = m.Lessons.Select(l => l.Id).ToList() })
            .ToListAsync(ct);

        // Get all lessons per enrolled course
        var enrolledCourseIds = await _coursesDb.CourseEnrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => e.CourseId)
            .ToListAsync(ct);

        var allCourseLessons = await _coursesDb.CourseModules
            .Include(m => m.Lessons)
            .Where(m => enrolledCourseIds.Contains(m.CourseId))
            .Select(m => new { m.CourseId, LessonIds = m.Lessons.Select(l => l.Id).ToList() })
            .ToListAsync(ct);

        var courseGroups = allCourseLessons
            .GroupBy(m => m.CourseId)
            .Select(g =>
            {
                var allLessonIds = g.SelectMany(m => m.LessonIds).ToList();
                var completedCount = allLessonIds.Count(lid => completedLessonIds.Contains(lid));
                var total = allLessonIds.Count;
                return new CourseProgressDto
                {
                    CourseId = g.Key,
                    TotalLessons = total,
                    CompletedLessons = completedCount,
                    TotalItems = total,
                    CompletedItems = completedCount,
                    ProgressPercent = total > 0 ? Math.Round((decimal)completedCount / total * 100, 2) : 0
                };
            })
            .ToList();

        return Ok(new MyProgressDto { Courses = courseGroups });
    }

    [HttpGet("lessons/{lessonId:guid}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetLessonProgress(Guid lessonId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await _lessonAccess.CanStudentAccessLessonAsync(lessonId, studentId, ct))
            return Forbid();

        var progress = await _progressDb.LessonProgresses
            .FirstOrDefaultAsync(p => p.LessonId == lessonId && p.StudentId == studentId, ct);

        return Ok(new LessonProgressDto
        {
            Id = progress?.Id ?? Guid.Empty,
            LessonId = lessonId,
            StudentId = studentId,
            IsCompleted = progress?.IsCompleted ?? false,
            CompletedAt = progress?.CompletedAt
        });
    }

    private async Task FillItemProgressAsync(
        CourseProgressDto progress,
        Guid courseId,
        string studentId,
        CancellationToken ct)
    {
        var itemIds = await _coursesDb.CourseItems
            .Where(i =>
                i.CourseId == courseId
                && i.IsRequired
                && i.Status != CourseItemStatus.Archived)
            .Select(i => i.Id)
            .ToListAsync(ct);

        progress.TotalItems = itemIds.Count;
        progress.CompletedItems = itemIds.Count == 0
            ? 0
            : await _progressDb.CourseItemProgresses
                .CountAsync(p =>
                    p.StudentId == studentId
                    && p.IsCompleted
                    && itemIds.Contains(p.CourseItemId), ct);

        if (progress.TotalItems > 0)
            progress.ProgressPercent = Math.Round((decimal)progress.CompletedItems / progress.TotalItems * 100, 2);
    }

    private async Task<bool> CanStudentAccessCourseAsync(Guid courseId, string studentId, CancellationToken ct)
    {
        return await _coursesDb.CourseEnrollments
            .AnyAsync(e =>
                e.CourseId == courseId
                && e.StudentId == studentId
                && e.Status != EnrollmentStatus.Dropped, ct);
    }

    private async Task SetCourseItemProgressBySourceAsync(
        string itemType,
        Guid sourceId,
        string studentId,
        bool isCompleted,
        CancellationToken ct)
    {
        var courseItemId = await _coursesDb.CourseItems
            .Where(i => i.Type.ToString() == itemType && i.SourceId == sourceId)
            .Select(i => (Guid?)i.Id)
            .FirstOrDefaultAsync(ct);

        if (courseItemId.HasValue)
            await SetCourseItemProgressAsync(courseItemId.Value, studentId, isCompleted, ct);
    }

    private async Task<(bool NotFound, bool Forbidden, CourseItemProgressDto? Progress)> SetCourseItemProgressAsync(
        Guid courseItemId,
        string studentId,
        bool isCompleted,
        CancellationToken ct)
    {
        var item = await _coursesDb.CourseItems
            .AsNoTracking()
            .Where(i => i.Id == courseItemId)
            .Select(i => new
            {
                i.Id,
                i.CourseId,
                i.SourceId,
                ItemType = i.Type.ToString()
            })
            .FirstOrDefaultAsync(ct);

        if (item is null)
            return (true, false, null);

        if (!await CanStudentAccessCourseAsync(item.CourseId, studentId, ct))
            return (false, true, null);

        var progress = await _progressDb.CourseItemProgresses
            .FirstOrDefaultAsync(p => p.CourseItemId == item.Id && p.StudentId == studentId, ct);

        if (progress is null)
        {
            progress = new CourseItemProgress
            {
                CourseId = item.CourseId,
                CourseItemId = item.Id,
                SourceId = item.SourceId,
                ItemType = item.ItemType,
                StudentId = studentId
            };
            _progressDb.CourseItemProgresses.Add(progress);
        }

        progress.IsCompleted = isCompleted;
        progress.CompletedAt = isCompleted ? DateTime.UtcNow : null;
        await _progressDb.SaveChangesAsync(ct);

        return (false, false, new CourseItemProgressDto
        {
            Id = progress.Id,
            CourseId = progress.CourseId,
            CourseItemId = progress.CourseItemId,
            SourceId = progress.SourceId,
            ItemType = progress.ItemType,
            StudentId = progress.StudentId,
            IsCompleted = progress.IsCompleted,
            CompletedAt = progress.CompletedAt
        });
    }
}
