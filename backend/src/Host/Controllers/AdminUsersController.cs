using Auth.Application.Commands.Admin.BlockUser;
using Auth.Application.Commands.Admin.ChangeUserRole;
using Auth.Application.Commands.Admin.CreateUser;
using Auth.Application.Commands.Admin.DeleteUser;
using Auth.Application.Commands.Admin.UnblockUser;
using Auth.Application.Queries.GetAllUsers;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] bool? onlyBlocked,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(search, role, onlyBlocked, page, pageSize), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "USERS_LIST_FAILED"))
            : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Email, request.FirstName, request.LastName, request.Role, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "USER_CREATE_FAILED"))
            : Ok(result.Value);
    }

    [HttpPost("{userId}/block")]
    public async Task<IActionResult> Block(string userId, CancellationToken cancellationToken)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actorUserId))
            return Unauthorized();

        var result = await _mediator.Send(new BlockUserCommand(userId, actorUserId), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "USER_BLOCK_FAILED"))
            : Ok(new { message = result.Value });
    }

    [HttpPost("{userId}/unblock")]
    public async Task<IActionResult> Unblock(string userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UnblockUserCommand(userId), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "USER_UNBLOCK_FAILED"))
            : Ok(new { message = result.Value });
    }

    [HttpPost("{userId}/role")]
    public async Task<IActionResult> ChangeRole(string userId, [FromBody] ChangeRoleRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actorUserId))
            return Unauthorized();

        var result = await _mediator.Send(new ChangeUserRoleCommand(userId, request.Role, actorUserId), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "USER_ROLE_FAILED"))
            : Ok(new { message = result.Value });
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string userId, CancellationToken cancellationToken)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actorUserId))
            return Unauthorized();

        var result = await _mediator.Send(new DeleteUserCommand(userId, actorUserId), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "USER_DELETE_FAILED"))
            : Ok(new { message = result.Value });
    }
}

public record CreateUserRequest(string Email, string FirstName, string LastName, string Role, string Password);
public record ChangeRoleRequest(string Role);
