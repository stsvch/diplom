using Auth.Application.Commands.UpdateProfile;
using Auth.Application.DTOs;
using Auth.Application.Queries.GetProfile;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetProfileQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "USER_NOT_FOUND"));

        return Ok(result.Value);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new UpdateProfileCommand(userId, request.FirstName, request.LastName, request.AvatarUrl);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.Error == "User not found."
                ? NotFound(ApiError.FromMessage(result.Error!, "USER_NOT_FOUND"))
                : BadRequest(ApiError.FromMessage(result.Error!, "USER_UPDATE_FAILED"));

        return Ok(result.Value);
    }
}

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? AvatarUrl);
