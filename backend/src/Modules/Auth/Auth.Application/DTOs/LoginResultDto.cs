namespace Auth.Application.DTOs;

public class LoginResultDto
{
    public AuthResponseDto AuthResponse { get; set; } = new();
    public string RefreshToken { get; set; } = string.Empty;
}
