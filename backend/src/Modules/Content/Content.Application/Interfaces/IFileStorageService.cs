namespace Content.Application.Interfaces;

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
