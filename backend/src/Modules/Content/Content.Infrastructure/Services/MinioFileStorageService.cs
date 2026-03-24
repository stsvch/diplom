using Content.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace Content.Infrastructure.Services;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private bool _bucketEnsured;

    public MinioFileStorageService(IMinioClient minioClient, IConfiguration configuration)
    {
        _minioClient = minioClient;
        _bucketName = configuration["MinIO:BucketName"] ?? "eduplatform-files";
    }

    public async Task<(string storagePath, string fileUrl)> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var sanitizedFileName = Path.GetFileName(fileName);
        var objectName = $"{Guid.NewGuid()}_{sanitizedFileName}";

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType),
            cancellationToken);

        var fileUrl = $"/api/files/{attachmentId}/download";

        return (objectName, fileUrl);
    }

    public async Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(storagePath),
                cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetDownloadUrlAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(storagePath)
            .WithExpiry(3600));

        return url;
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        if (_bucketEnsured)
            return;

        var exists = await _minioClient.BucketExistsAsync(new BucketExistsArgs()
            .WithBucket(_bucketName),
            cancellationToken);

        if (!exists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs()
                .WithBucket(_bucketName),
                cancellationToken);
        }

        _bucketEnsured = true;
    }
}
