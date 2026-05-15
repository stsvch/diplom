// Файл: MinioGlossaryImageStorage.cs
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Tools.Application.Interfaces;

namespace Tools.Infrastructure.Services;

// Сервис MinioGlossaryImageStorage инкапсулирует прикладную операцию и скрывает детали инфраструктуры.
public class MinioGlossaryImageStorage : IGlossaryImageStorage
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private bool _bucketEnsured;

    public MinioGlossaryImageStorage(IMinioClient minioClient, IConfiguration configuration)
    {
        _minioClient = minioClient;
        _bucketName = configuration["MinIO:BucketName"] ?? "eduplatform-files";
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var safeName = Path.GetFileName(fileName);
        var objectName = $"glossary/{Guid.NewGuid():N}_{safeName}";

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType),
            cancellationToken);

        return objectName;
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(storageKey),
                cancellationToken);
        }
        catch
        {
        }
    }

    public async Task<string> GetPresignedUrlAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        return await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(storageKey)
            .WithExpiry(3600));
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
