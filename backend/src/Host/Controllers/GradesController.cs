using Auth.Domain.Entities;
using Courses.Application.Interfaces;
using EduPlatform.Shared.Application.Models;
using Grading.Application.DTOs;
using Grading.Application.Grades.Commands.CreateGrade;
using Grading.Application.Grades.Commands.DeleteGrade;
using Grading.Application.Grades.Commands.UpdateGrade;
using Grading.Application.Grades.Queries.GetCourseGradebook;
using Grading.Application.Grades.Queries.GetGradebookStats;
using Grading.Application.Grades.Queries.GetStudentGrades;
using Grading.Application.Interfaces;
using Grading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/grades")]
public class GradesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICoursesDbContext _coursesDb;
    private readonly IExportService _excelExportService;
    private readonly Grading.Infrastructure.Services.PdfExportService _pdfExportService;

    public GradesController(
        IMediator mediator,
        UserManager<ApplicationUser> userManager,
        ICoursesDbContext coursesDb,
        IExportService excelExportService,
        Grading.Infrastructure.Services.PdfExportService pdfExportService)
    {
        _mediator = mediator;
        _userManager = userManager;
        _coursesDb = coursesDb;
        _excelExportService = excelExportService;
        _pdfExportService = pdfExportService;
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create([FromBody] CreateGradeRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new CreateGradeCommand(
            request.StudentId,
            request.CourseId,
            request.SourceType,
            request.TestAttemptId,
            request.AssignmentSubmissionId,
            request.Title,
            request.Score,
            request.MaxScore,
            request.Comment,
            DateTime.UtcNow,
            userId);

        var result = await _mediator.Send(command, ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "GRADE_CREATE_FAILED"));
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGradeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateGradeCommand(id, request.Score, request.MaxScore, request.Comment), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "GRADE_UPDATE_FAILED"));
        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new DeleteGradeCommand(id, userId), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "GRADE_DELETE_FAILED"));
        return Ok(new { message = "Grade deleted" });
    }

    [HttpGet("course/{courseId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetCourseGradebook(Guid courseId, CancellationToken ct)
    {
        var gradebook = await _mediator.Send(new GetCourseGradebookQuery(courseId), ct);
        await EnrichGradebookAsync(gradebook, ct);
        return Ok(gradebook);
    }

    [HttpGet("course/{courseId:guid}/stats")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetGradebookStats(Guid courseId, CancellationToken ct)
    {
        var stats = await _mediator.Send(new GetGradebookStatsQuery(courseId), ct);
        return Ok(stats);
    }

    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> GetStudentGrades(string studentId, CancellationToken ct)
    {
        var grades = await _mediator.Send(new GetStudentGradesQuery(studentId), ct);
        await EnrichGradesAsync(grades, ct);
        return Ok(grades);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyGrades(CancellationToken ct)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var grades = await _mediator.Send(new GetStudentGradesQuery(studentId), ct);
        await EnrichGradesAsync(grades, ct);
        return Ok(grades);
    }

    [HttpGet("course/{courseId:guid}/export/excel")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ExportExcel(Guid courseId, CancellationToken ct)
    {
        var gradebook = await _mediator.Send(new GetCourseGradebookQuery(courseId), ct);
        await EnrichGradebookAsync(gradebook, ct);
        if (!HasGrades(gradebook))
            return NotFound(ApiError.FromMessage("В журнале нет данных для экспорта.", "GRADEBOOK_EMPTY"));

        var bytes = await _excelExportService.ExportToExcelAsync(gradebook, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"gradebook_{courseId:N}.xlsx");
    }

    [HttpGet("course/{courseId:guid}/export/pdf")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ExportPdf(Guid courseId, CancellationToken ct)
    {
        var gradebook = await _mediator.Send(new GetCourseGradebookQuery(courseId), ct);
        await EnrichGradebookAsync(gradebook, ct);
        if (!HasGrades(gradebook))
            return NotFound(ApiError.FromMessage("В журнале нет данных для экспорта.", "GRADEBOOK_EMPTY"));

        var bytes = await _pdfExportService.ExportToPdfAsync(gradebook, ct);
        return File(bytes, "application/pdf", $"gradebook_{courseId:N}.pdf");
    }

    private async Task EnrichGradebookAsync(GradebookDto gradebook, CancellationToken ct)
    {
        var courseName = await _coursesDb.Courses
            .Where(c => c.Id == gradebook.CourseId)
            .Select(c => c.Title)
            .FirstOrDefaultAsync(ct);

        gradebook.CourseName = courseName ?? string.Empty;

        var usersById = await LoadUserNamesAsync(
            gradebook.Students.Select(s => s.StudentId),
            ct);

        foreach (var student in gradebook.Students)
        {
            student.StudentName = usersById.TryGetValue(student.StudentId, out var fullName)
                ? fullName
                : student.StudentName;
        }
    }

    private async Task EnrichGradesAsync(List<GradeDto> grades, CancellationToken ct)
    {
        var courseIds = grades
            .Select(g => g.CourseId)
            .Distinct()
            .ToList();

        if (courseIds.Count == 0)
            return;

        var courses = await _coursesDb.Courses
            .Where(c => courseIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Title })
            .ToDictionaryAsync(c => c.Id, c => c.Title, ct);

        foreach (var grade in grades)
        {
            if (courses.TryGetValue(grade.CourseId, out var courseName))
            {
                grade.CourseName = courseName;
            }
        }
    }

    private async Task<Dictionary<string, string>> LoadUserNamesAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        var ids = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return new Dictionary<string, string>();

        return await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.UserName })
            .ToDictionaryAsync(
                u => u.Id,
                u =>
                {
                    var fullName = $"{u.FirstName} {u.LastName}".Trim();
                    if (!string.IsNullOrWhiteSpace(fullName))
                        return fullName;

                    return u.Email ?? u.UserName ?? u.Id;
                },
                ct);
    }

    private static bool HasGrades(GradebookDto gradebook) =>
        gradebook.Students.Any(student => student.Grades.Count > 0);
}

public record CreateGradeRequest(
    string StudentId,
    Guid CourseId,
    GradeSourceType SourceType,
    Guid? TestAttemptId,
    Guid? AssignmentSubmissionId,
    string Title,
    decimal Score,
    decimal MaxScore,
    string? Comment);

public record UpdateGradeRequest(decimal Score, decimal MaxScore, string? Comment);
