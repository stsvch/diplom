// Файл: IGlossaryImageStorage.cs
namespace Tools.Application.Interfaces;

// Интерфейс IGlossaryImageStorage задаёт контракт сервиса между слоями или модулями.
public interface IGlossaryImageStorage
{
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);

    Task<string> GetPresignedUrlAsync(string storageKey, CancellationToken cancellationToken = default);
}
