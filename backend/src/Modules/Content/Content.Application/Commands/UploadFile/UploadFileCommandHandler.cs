// UploadFileCommandHandler.cs

using AutoMapper;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using Content.Domain.Entities;
using Content.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Commands.UploadFile;

// Тип class: ключевой элемент файла UploadFileCommandHandler.cs.
public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<AttachmentDto>>
{
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".sh", ".cmd", ".ps1", ".msi"
    };

    private const long MaxFileSizeBytes = 100L * 1024 * 1024;        // 100 MB
    private const long MaxVideoFileSizeBytes = 1024L * 1024 * 1024;  // 1 GB

    private static readonly HashSet<string> VideoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/mpeg", "video/ogg", "video/webm", "video/quicktime",
        "video/x-msvideo", "video/x-matroska", "video/x-flv"
    };

    private readonly IContentDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public UploadFileCommandHandler(
        IContentDbContext context,
        IFileStorageService fileStorageService,
        IMapper mapper)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<AttachmentDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // Проверяем расширение файла.
        var extension = Path.GetExtension(request.FileName);
        if (BlockedExtensions.Contains(extension))
            return Result.Failure<AttachmentDto>($"File extension '{extension}' is not allowed.");

        // Проверяем размер файла.
        var isVideo = VideoContentTypes.Contains(request.ContentType);
        var maxSize = isVideo ? MaxVideoFileSizeBytes : MaxFileSizeBytes;
        if (request.FileSize > maxSize)
        {
            var limitMb = maxSize / (1024 * 1024);
            return Result.Failure<AttachmentDto>($"File size exceeds the maximum allowed size of {limitMb} MB.");
        }

        // Разбираем тип связанной сущности.
        if (!Enum.TryParse<AttachmentEntityType>(request.EntityType, ignoreCase: true, out var entityType))
            return Result.Failure<AttachmentDto>($"Unknown entity type: '{request.EntityType}'.");

        // EntityId необязателен только для сущностей без Guid, например сообщений чата в MongoDB.
        // Также он может отсутствовать, если родитель еще не создан в момент загрузки.
        var allowsNullEntityId = entityType is AttachmentEntityType.ChatMessage or AttachmentEntityType.UserAvatar;
        if (!allowsNullEntityId && request.EntityId is null)
            return Result.Failure<AttachmentDto>($"entityId is required for entity type '{entityType}'.");

        // Создаем Attachment до загрузки, чтобы получить Id для FileUrl.
        var attachment = new Attachment
        {
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            EntityType = entityType,
            EntityId = request.EntityId,
            UploadedById = request.UploadedById,
            StoragePath = string.Empty,
            FileUrl = string.Empty
        };

        // Загружаем файл в хранилище.
        var (storagePath, fileUrl) = await _fileStorageService.UploadAsync(
            request.Stream,
            request.FileName,
            request.ContentType,
            attachment.Id,
            cancellationToken);

        attachment.StoragePath = storagePath;
        attachment.FileUrl = fileUrl;

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<AttachmentDto>(attachment));
    }
}
