// RefreshToken.cs
namespace Auth.Domain.Entities;

// Refresh-токен пользовательской сессии: хранит значение токена, владельца, срок действия и флаг отзыва для logout/rotation/block.
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;

    public ApplicationUser? User { get; set; }
}
