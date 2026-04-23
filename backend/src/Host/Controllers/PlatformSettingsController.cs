using Auth.Application.Commands.UpdatePlatformSettings;
using Auth.Application.DTOs;
using Auth.Application.Queries.GetPlatformSettings;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/platform-settings")]
public class PlatformSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Public: returns only non-sensitive flags (for landing / auth pages)
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlatformSettingsQuery(), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "SETTINGS_FAILED"))
            : Ok(result.Value);
    }

    // Admin full view
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlatformSettingsQuery(), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "SETTINGS_FAILED"))
            : Ok(result.Value);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdatePlatformSettingsRequest request, CancellationToken cancellationToken)
    {
        var cmd = new UpdatePlatformSettingsCommand(
            request.RegistrationOpen,
            request.MaintenanceMode,
            request.PlatformName,
            request.SupportEmail);
        var result = await _mediator.Send(cmd, cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "SETTINGS_UPDATE_FAILED"))
            : Ok(result.Value);
    }
}

public record UpdatePlatformSettingsRequest(
    bool RegistrationOpen,
    bool MaintenanceMode,
    string PlatformName,
    string SupportEmail);
