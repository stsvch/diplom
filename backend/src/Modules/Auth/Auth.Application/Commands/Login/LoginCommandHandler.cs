using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IAuthDbContext _dbContext;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IAuthDbContext dbContext)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure<LoginResultDto>("Invalid email or password.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Result.Failure<LoginResultDto>("Invalid email or password.");

        if (!user.EmailConfirmed)
            return Result.Failure<LoginResultDto>("Please confirm your email before logging in.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new Auth.Domain.Entities.RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var loginResult = new LoginResultDto
        {
            AuthResponse = new AuthResponseDto
            {
                AccessToken = accessToken,
                ExpiresAt = expiresAt
            },
            RefreshToken = refreshTokenValue
        };

        return Result.Success(loginResult);
    }
}
