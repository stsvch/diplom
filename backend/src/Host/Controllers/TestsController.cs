using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tests.Application.DTOs;
using Tests.Application.Tests.Commands.CreateTest;
using Tests.Application.Tests.Commands.DeleteTest;
using Tests.Application.Tests.Commands.UpdateTest;
using Tests.Application.Tests.Queries.GetTestById;
using Tests.Application.Tests.Queries.GetTestSubmissions;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/tests")]
public class TestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(TestDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTestRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new CreateTestCommand(
            request.Title,
            request.Description,
            userId,
            request.TimeLimitMinutes,
            request.MaxAttempts,
            request.ShuffleQuestions,
            request.ShuffleAnswers,
            request.ShowCorrectAnswers);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "TEST_CREATE_FAILED"));

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TestDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTestByIdQuery(id), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "TEST_NOT_FOUND"));

        var test = result.Value!;
        var role = User.FindFirstValue(ClaimTypes.Role);

        // Скрыть правильные ответы от студентов
        if (role == "Student")
        {
            foreach (var q in test.Questions)
            {
                foreach (var opt in q.AnswerOptions)
                {
                    opt.IsCorrect = false;
                }
            }
        }

        return Ok(test);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(TestDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTestRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new UpdateTestCommand(
            id,
            userId,
            request.Title,
            request.Description,
            request.TimeLimitMinutes,
            request.MaxAttempts,
            request.Deadline,
            request.ShuffleQuestions,
            request.ShuffleAnswers,
            request.ShowCorrectAnswers);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "TEST_UPDATE_FAILED"));

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new DeleteTestCommand(id, userId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "TEST_DELETE_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpGet("{id:guid}/submissions")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(List<TestAttemptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubmissions(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetTestSubmissionsQuery(id, userId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "TEST_SUBMISSIONS_FAILED"));

        return Ok(result.Value);
    }
}

public record CreateTestRequest(
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    bool ShuffleQuestions,
    bool ShuffleAnswers,
    bool ShowCorrectAnswers);

public record UpdateTestRequest(
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    DateTime? Deadline,
    bool ShuffleQuestions,
    bool ShuffleAnswers,
    bool ShowCorrectAnswers);
