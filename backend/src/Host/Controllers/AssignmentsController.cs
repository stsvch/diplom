using Assignments.Application.Assignments.Commands.CreateAssignment;
using Assignments.Application.Assignments.Commands.DeleteAssignment;
using Assignments.Application.Assignments.Commands.GradeSubmission;
using Assignments.Application.Assignments.Commands.SubmitAssignment;
using Assignments.Application.Assignments.Commands.UpdateAssignment;
using Assignments.Application.Assignments.Queries.GetAssignmentById;
using Assignments.Application.Assignments.Queries.GetMySubmissions;
using Assignments.Application.Assignments.Queries.GetPendingSubmissions;
using Assignments.Application.Assignments.Queries.GetSubmissions;
using Assignments.Application.DTOs;
using Assignments.Domain.Enums;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api")]
public class AssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AssignmentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("assignments")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new CreateAssignmentCommand(request.Title, request.Description, request.Criteria,
            request.Deadline, request.MaxAttempts, request.MaxScore, userId);
        var result = await _mediator.Send(command, ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "ASSIGNMENT_CREATE_FAILED"));
        return Ok(result.Value);
    }

    [HttpGet("assignments/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAssignmentByIdQuery(id), ct);
        if (result.IsFailure) return NotFound(ApiError.FromMessage(result.Error!, "ASSIGNMENT_NOT_FOUND"));
        return Ok(result.Value);
    }

    [HttpPut("assignments/{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssignmentRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new UpdateAssignmentCommand(id, userId, request.Title, request.Description,
            request.Criteria, request.Deadline, request.MaxAttempts, request.MaxScore);
        var result = await _mediator.Send(command, ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "ASSIGNMENT_UPDATE_FAILED"));
        return Ok(result.Value);
    }

    [HttpDelete("assignments/{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new DeleteAssignmentCommand(id, userId), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "ASSIGNMENT_DELETE_FAILED"));
        return Ok(new { message = result.Value });
    }

    [HttpGet("assignments/{id:guid}/submissions")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetSubmissions(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _mediator.Send(new GetSubmissionsQuery(id, userId), ct));
    }

    [HttpGet("assignments/{id:guid}/my-submissions")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMySubmissions(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _mediator.Send(new GetMySubmissionsQuery(id, userId), ct));
    }

    [HttpPost("assignments/{id:guid}/submit")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new SubmitAssignmentCommand(id, userId, request.Content), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "SUBMISSION_FAILED"));
        return Ok(result.Value);
    }

    [HttpPut("submissions/{id:guid}/grade")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Grade(Guid id, [FromBody] GradeRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var status = request.ReturnForRevision ? SubmissionStatus.ReturnedForRevision : SubmissionStatus.Graded;
        var result = await _mediator.Send(new GradeSubmissionCommand(id, userId, request.Score, request.Comment, status), ct);
        if (result.IsFailure) return BadRequest(ApiError.FromMessage(result.Error!, "GRADE_FAILED"));
        return Ok(new { message = result.Value });
    }

    [HttpGet("assignments/pending")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _mediator.Send(new GetPendingSubmissionsQuery(userId), ct));
    }
}

public record CreateAssignmentRequest(string Title, string Description, string? Criteria, DateTime? Deadline, int? MaxAttempts, int MaxScore);
public record UpdateAssignmentRequest(string Title, string Description, string? Criteria, DateTime? Deadline, int? MaxAttempts, int MaxScore);
public record SubmitRequest(string? Content);
public record GradeRequest(int Score, string? Comment, bool ReturnForRevision = false);
