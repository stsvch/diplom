// LoginResultDto.cs
namespace Auth.Application.DTOs;

// Внутренний результат login/refresh: AuthResponse уходит клиенту в JSON, а RefreshToken контроллер кладёт в httpOnly cookie.
public class LoginResultDto
{
    public AuthResponseDto AuthResponse { get; set; } = new();
    public string RefreshToken { get; set; } = string.Empty;
}
