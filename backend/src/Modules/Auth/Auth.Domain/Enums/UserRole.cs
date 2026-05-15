// UserRole.cs
namespace Auth.Domain.Enums;

// Роли определяют основной контур доступа: Student учится, Teacher создаёт и проверяет материалы, Admin управляет платформой.
public enum UserRole
{
    Admin = 0,
    Teacher = 1,
    Student = 2
}
