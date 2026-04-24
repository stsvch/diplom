using Auth.Application.Queries.GetDashboardStats;
using Courses.Application.Courses.Queries.GetCourseStats;
using EduPlatform.Host.Services;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Roles = "Admin")]
public class AdminStatsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AdminAnalyticsReadService _analytics;

    public AdminStatsController(IMediator mediator, AdminAnalyticsReadService analytics)
    {
        _mediator = mediator;
        _analytics = analytics;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(new GetUserStatsQuery(), cancellationToken);
        if (users.IsFailure)
            return BadRequest(ApiError.FromMessage(users.Error!, "STATS_USERS_FAILED"));

        var courses = await _mediator.Send(new GetCourseStatsQuery(), cancellationToken);
        if (courses.IsFailure)
            return BadRequest(ApiError.FromMessage(courses.Error!, "STATS_COURSES_FAILED"));

        return Ok(new
        {
            users = users.Value,
            courses = courses.Value,
        });
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> Analytics(CancellationToken cancellationToken)
    {
        var analytics = await _analytics.GetAsync(cancellationToken);
        return Ok(analytics);
    }
}
