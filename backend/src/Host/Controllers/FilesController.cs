using Content.Application.Commands.DeleteFile;
using Content.Application.Commands.UploadFile;
using Content.Application.DTOs;
using Content.Application.Queries.GetDownloadUrl;
using Content.Application.Queries.GetEntityFiles;
using Content.Application.Queries.GetFileInfo;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(1_073_741_824)]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] string entityType,
        [FromForm] Guid entityId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(ApiError.FromMessage("No file provided.", "FILE_MISSING"));

        await using var stream = file.OpenReadStream();
        var command = new UploadFileCommand
        {
            Stream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            EntityType = entityType,
            EntityId = entityId,
            UploadedById = userId
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "FILE_UPLOAD_FAILED"));

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFileInfoQuery(id), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "FILE_NOT_FOUND"));

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDownloadUrlQuery(id), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "FILE_NOT_FOUND"));

        return Redirect(result.Value!);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new DeleteFileCommand(id, userId), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found"))
                return NotFound(ApiError.FromMessage(result.Error, "FILE_NOT_FOUND"));
            return BadRequest(ApiError.FromMessage(result.Error, "FILE_DELETE_FAILED"));
        }

        return Ok(new { message = result.Value });
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<AttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEntityFiles(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(entityType))
            return BadRequest(ApiError.FromMessage("entityType is required.", "ENTITY_TYPE_REQUIRED"));

        var result = await _mediator.Send(new GetEntityFilesQuery(entityType, entityId), cancellationToken);
        return Ok(result);
    }
}
