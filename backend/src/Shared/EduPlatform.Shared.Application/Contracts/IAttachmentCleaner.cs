// Файл: IAttachmentCleaner.cs
namespace EduPlatform.Shared.Application.Contracts;

// Интерфейс IAttachmentCleaner задаёт контракт сервиса между слоями или модулями.
public interface IAttachmentCleaner
{
    Task DeleteAsync(IEnumerable<Guid> attachmentIds, CancellationToken cancellationToken = default);
}
