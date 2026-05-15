// ITokenService.cs
using System.Security.Claims;
using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

// Контракт JWT/refresh-token логики: создаёт access token, генерирует refresh token и извлекает principal из истёкшего токена.
public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
