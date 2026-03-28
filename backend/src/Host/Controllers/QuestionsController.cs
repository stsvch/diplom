using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tests.Application.DTOs;
using Tests.Application.Tests.Commands.AddQuestion;
using Tests.Application.Tests.Commands.DeleteQuestion;
using Tests.Application.Tests.Commands.ReorderQuestions;
using Tests.Application.Tests.Commands.UpdateQuestion;
using Tests.Domain.Enums;

namespace EduPlatform.Host.Controllers;

[ApiController]
public class QuestionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("api/tests/{testId:guid}/questions")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddQuestion(Guid testId, [FromBody] AddQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new AddQuestionCommand(
            testId,
            userId,
            request.Type,
            request.Text,
            request.Points,
            request.AnswerOptions.Select(o => new AnswerOptionInput(o.Text, o.IsCorrect, o.MatchingPairValue)).ToList());

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "QUESTION_ADD_FAILED"));

        return Created($"/api/questions/{result.Value!.Id}", result.Value);
    }

    [HttpPut("api/questions/{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new UpdateQuestionCommand(
            id,
            userId,
            request.Type,
            request.Text,
            request.Points,
            request.AnswerOptions.Select(o => new UpdateAnswerOptionInput(o.Id, o.Text, o.IsCorrect, o.MatchingPairValue)).ToList());

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "QUESTION_UPDATE_FAILED"));

        return Ok(result.Value);
    }

    [HttpDelete("api/questions/{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteQuestion(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new DeleteQuestionCommand(id, userId), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "QUESTION_DELETE_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("api/tests/{testId:guid}/questions/reorder")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderQuestions(Guid testId, [FromBody] ReorderQuestionsRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new ReorderQuestionsCommand(testId, userId, request.OrderedIds);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "QUESTIONS_REORDER_FAILED"));

        return Ok(new { message = result.Value });
    }
}

public record AddQuestionRequest(
    QuestionType Type,
    string Text,
    int Points,
    List<AnswerOptionRequest> AnswerOptions);

public record AnswerOptionRequest(string Text, bool IsCorrect, string? MatchingPairValue);

public record UpdateQuestionRequest(
    QuestionType Type,
    string Text,
    int Points,
    List<UpdateAnswerOptionRequest> AnswerOptions);

public record UpdateAnswerOptionRequest(Guid? Id, string Text, bool IsCorrect, string? MatchingPairValue);

public record ReorderQuestionsRequest(List<Guid> OrderedIds);
