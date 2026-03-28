using Courses.Application.Interfaces;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Progress.Application.DTOs;
using Progress.Application.Interfaces;
using Progress.Application.Progress.Commands.CompleteLesson;
using Progress.Application.Progress.Commands.UncompleteLesson;
using Progress.Application.Progress.Queries.GetCourseProgress;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/progress")]
public class ProgressController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICoursesDbContext _coursesDb;
    private readonly IProgressDbContext _progressDb;

    public ProgressController(
        IMediator mediator,
        ICoursesDbContext coursesDb,
        IProgressDbContext progressDb)
    {
        _mediator = mediator;
        _coursesDb = coursesDb;
        _progressDb = progressDb;
    }

    [HttpPost("lessons/{lessonId:guid}/complete")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CompleteLesson(Guid lessonId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new CompleteLessonCommand(lessonId, studentId), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "COMPLETE_FAILED"));
        return Ok(result.Value);
    }

    [HttpDelete("lessons/{lessonId:guid}/complete")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UncompleteLesson(Guid lessonId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new UncompleteLessonCommand(lessonId, studentId), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "UNCOMPLETE_FAILED"));
        return Ok(new { message = "Lesson marked as not completed" });
    }

    [HttpGet("courses/{courseId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetCourseProgress(Guid courseId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var lessonIds = await _coursesDb.CourseModules
            .Where(m => m.CourseId == courseId)
            .SelectMany(m => m.Lessons.Select(l => l.Id))
            .ToListAsync(ct);

        var progress = await _mediator.Send(new GetCourseProgressQuery(courseId, studentId, lessonIds), ct);
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
                    ProgressPercent = total > 0 ? Math.Round((decimal)completedCount / total * 100, 2) : 0
                };
            })
            .ToList();

        return Ok(new MyProgressDto { Courses = courseGroups });
    }

    [HttpGet("lessons/{lessonId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetLessonProgress(Guid lessonId, CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
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
}
