// AuthResponseDto.cs
namespace Auth.Application.DTOs;

// Ответ, который клиент получает в body после login/refresh: хранит accessToken для Authorization header и ExpiresAt для понимания срока жизни токена.
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
