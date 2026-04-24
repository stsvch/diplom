using EduPlatform.Shared.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tools.Application.DTOs;
using Tools.Application.Interfaces;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/glossary")]
public class GlossaryController : ControllerBase
{
    private readonly IGlossaryService _glossaryService;

    public GlossaryController(IGlossaryService glossaryService)
    {
        _glossaryService = glossaryService;
    }

    [HttpGet("words")]
    [Authorize]
    [ProducesResponseType(typeof(List<DictionaryWordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWords(
        [FromQuery] Guid? courseId,
        [FromQuery] string? search,
        [FromQuery] bool knownOnly = false,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (User.IsInRole("Teacher"))
        {
            var result = await _glossaryService.GetTeacherWordsAsync(userId, courseId, search, cancellationToken);
            return Ok(result);
        }

        if (User.IsInRole("Student"))
        {
            var result = await _glossaryService.GetStudentWordsAsync(userId, courseId, search, knownOnly, cancellationToken);
            return Ok(result);
        }

        return Forbid();
    }

    [HttpPost("words")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(DictionaryWordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWord([FromBody] UpsertDictionaryWordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _glossaryService.CreateWordAsync(
                userId,
                request.CourseId,
                request.Term,
                request.Translation,
                request.Definition,
                request.Example,
                request.Tags,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiError.FromMessage(ex.Message, "GLOSSARY_COURSE_NOT_FOUND"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "GLOSSARY_CREATE_FAILED"));
        }
    }

    [HttpPut("words/{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(DictionaryWordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateWord(Guid id, [FromBody] UpsertDictionaryWordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _glossaryService.UpdateWordAsync(
                id,
                userId,
                request.CourseId,
                request.Term,
                request.Translation,
                request.Definition,
                request.Example,
                request.Tags,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiError.FromMessage(ex.Message, "GLOSSARY_WORD_NOT_FOUND"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "GLOSSARY_UPDATE_FAILED"));
        }
    }

    [HttpDelete("words/{id:guid}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWord(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            await _glossaryService.DeleteWordAsync(id, userId, cancellationToken);
            return Ok(new { message = "Слово удалено." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiError.FromMessage(ex.Message, "GLOSSARY_WORD_NOT_FOUND"));
        }
    }

    [HttpPost("words/{id:guid}/progress")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(DictionaryWordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetProgress(Guid id, [FromBody] UpdateDictionaryWordProgressRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _glossaryService.SetStudentProgressAsync(id, userId, request.IsKnown, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiError.FromMessage(ex.Message, "GLOSSARY_WORD_NOT_FOUND"));
        }
    }

    [HttpPost("review-session")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(List<DictionaryWordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewSession(
        [FromBody] ReviewSessionRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _glossaryService.GetStudentReviewSessionAsync(
            userId,
            request?.CourseId,
            request?.Take ?? 12,
            request?.ExcludeWordIds,
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("words/{id:guid}/review")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(DictionaryWordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewWord(Guid id, [FromBody] ReviewDictionaryWordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _glossaryService.ReviewWordAsync(id, userId, request.Outcome, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiError.FromMessage(ex.Message, "GLOSSARY_WORD_NOT_FOUND"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "GLOSSARY_REVIEW_FAILED"));
        }
    }
}

public record UpsertDictionaryWordRequest(
    Guid CourseId,
    string Term,
    string Translation,
    string? Definition,
    string? Example,
    List<string>? Tags);

public record UpdateDictionaryWordProgressRequest(bool IsKnown);

public record ReviewDictionaryWordRequest(string Outcome);

public record ReviewSessionRequest(Guid? CourseId, int Take = 12, List<Guid>? ExcludeWordIds = null);
