using Auth.Application.Commands.ConfirmEmail;
using Auth.Application.Commands.ForgotPassword;
using Auth.Application.Commands.Login;
using Auth.Application.Commands.Logout;
using Auth.Application.Commands.RefreshToken;
using Auth.Application.Commands.Register;
using Auth.Application.Commands.ResetPassword;
using Auth.Application.DTOs;
using Auth.Domain.Enums;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;
    private const string RefreshTokenCookieName = "refreshToken";
    private const string RefreshTokenCookiePath = "/api/auth";

    public AuthController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Role);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "AUTH_REGISTER_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token, CancellationToken cancellationToken)
    {
        var command = new ConfirmEmailCommand(userId, token);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "AUTH_CONFIRM_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "AUTH_LOGIN_FAILED"));

        var loginResult = result.Value!;

        SetRefreshTokenCookie(loginResult.RefreshToken);

        return Ok(loginResult.AuthResponse);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshAccessTokenRequest request, CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(ApiError.FromMessage("Refresh token отсутствует.", "AUTH_REFRESH_MISSING"));

        var command = new RefreshTokenCommand(request.AccessToken, refreshToken);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "AUTH_REFRESH_FAILED"));

        var loginResult = result.Value!;

        SetRefreshTokenCookie(loginResult.RefreshToken);

        return Ok(loginResult.AuthResponse);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var command = new LogoutCommand(userId);
            await _mediator.Send(command, cancellationToken);
        }

        ClearRefreshTokenCookie();

        return Ok(new { message = "Вы вышли из системы." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "AUTH_FORGOT_FAILED"));

        return Ok(new { message = result.Value });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "AUTH_RESET_FAILED"));

        return Ok(new { message = result.Value });
    }

    private CookieOptions CreateCookieOptions(DateTimeOffset expires)
    {
        var isDev = _env.IsDevelopment();
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.Strict,
            Expires = expires,
            Path = RefreshTokenCookiePath
        };
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken,
            CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(7)));
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Append(RefreshTokenCookieName, string.Empty,
            CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(-1)));
    }
}

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    UserRole Role);

public record RefreshAccessTokenRequest(string AccessToken);
