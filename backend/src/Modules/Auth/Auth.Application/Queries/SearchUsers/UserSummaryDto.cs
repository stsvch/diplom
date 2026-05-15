// UserSummaryDto.cs
namespace Auth.Application.Queries.SearchUsers;

// Короткая карточка пользователя для поиска/селектов: id, ФИО, email и роль без лишних полей безопасности.
public class UserSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
