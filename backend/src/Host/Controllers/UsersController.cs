using Auth.Application.Commands.ChangePassword;
using Auth.Application.Commands.UpdateProfile;
using Auth.Application.DTOs;
using Auth.Application.Queries.GetProfile;
using Auth.Application.Queries.SearchUsers;
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

    [HttpGet("search")]
    [ProducesResponseType(typeof(List<UserSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] string? role,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(new SearchUsersQuery(q, role, callerId, limit), cancellationToken);
        return result.IsFailure ? BadRequest(ApiError.FromMessage(result.Error!, "SEARCH_FAILED")) : Ok(result.Value);
    }

    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "PASSWORD_CHANGE_FAILED"));

        return Ok(new { message = result.Value });
    }
}

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? AvatarUrl);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
