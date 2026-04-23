using System.Security.Claims;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Auth.Application.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResultDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IAuthDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IAuthDbContext dbContext,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<Result<LoginResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        ClaimsPrincipal principal;
        try
        {
            principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        }
        catch
        {
            return Result.Failure<LoginResultDto>("Invalid access token.");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Result.Failure<LoginResultDto>("Invalid access token.");

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId, cancellationToken);

        if (storedToken == null)
            return Result.Failure<LoginResultDto>("Invalid refresh token.");

        if (storedToken.IsRevoked)
            return Result.Failure<LoginResultDto>("Refresh token has been revoked.");

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
            return Result.Failure<LoginResultDto>("Refresh token has expired.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<LoginResultDto>("User not found.");

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            var activeTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
                token.IsRevoked = true;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Failure<LoginResultDto>("Ваш аккаунт заблокирован. Обратитесь к администратору.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

        storedToken.IsRevoked = true;

        var newRefreshToken = new Auth.Domain.Entities.RefreshToken
        {
            Token = newRefreshTokenValue,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var expirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
        var loginResult = new LoginResultDto
        {
            AuthResponse = new AuthResponseDto
            {
                AccessToken = newAccessToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            },
            RefreshToken = newRefreshTokenValue
        };

        return Result.Success(loginResult);
    }
}
