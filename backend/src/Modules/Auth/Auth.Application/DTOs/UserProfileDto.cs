// UserProfileDto.cs
namespace Auth.Application.DTOs;

// Профиль пользователя для frontend: id, email, имя, фамилия, avatarUrl, роль и признак подтверждения email.
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
}
