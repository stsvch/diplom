// Файл: IAuditableEntity.cs
namespace EduPlatform.Shared.Domain;

// Интерфейс IAuditableEntity задаёт контракт сервиса между слоями или модулями.
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
