// IFileStorageService.cs

namespace Content.Application.Interfaces;

// Контракт interface: задаёт границу между слоями без привязки application layer к инфраструктуре.
public interface IFileStorageService
{
    Task<(string storagePath, string fileUrl)> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        Guid attachmentId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default);

    Task<string> GetDownloadUrlAsync(string storagePath, CancellationToken cancellationToken = default);
}
