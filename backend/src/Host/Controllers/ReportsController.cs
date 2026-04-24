using EduPlatform.Host.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly StudentDashboardReadService _studentDashboard;
    private readonly TeacherDashboardReadService _teacherDashboard;
    private readonly TeacherCourseReportReadService _teacherCourseReport;

    public ReportsController(
        StudentDashboardReadService studentDashboard,
        TeacherDashboardReadService teacherDashboard,
        TeacherCourseReportReadService teacherCourseReport)
    {
        _studentDashboard = studentDashboard;
        _teacherDashboard = teacherDashboard;
        _teacherCourseReport = teacherCourseReport;
    }

    [HttpGet("student/dashboard")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStudentDashboard(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        var dashboard = await _studentDashboard.GetAsync(studentId, cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("teacher/dashboard")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetTeacherDashboard(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        var dashboard = await _teacherDashboard.GetAsync(teacherId, cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("teacher/courses/{courseId:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetTeacherCourseReport(Guid courseId, CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        var report = await _teacherCourseReport.GetAsync(teacherId, courseId, cancellationToken);
        if (report is null)
            return NotFound();

        return Ok(report);
    }
}
