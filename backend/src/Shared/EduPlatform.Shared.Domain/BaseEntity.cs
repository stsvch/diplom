// Файл: BaseEntity.cs
namespace EduPlatform.Shared.Domain;

// Базовая сущность BaseEntity задаёт общие поля для доменных моделей.
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
