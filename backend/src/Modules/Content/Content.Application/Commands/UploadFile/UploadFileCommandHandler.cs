using AutoMapper;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using Content.Domain.Entities;
using Content.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Commands.UploadFile;

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

    public async Task<Result<AttachmentDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // Validate extension
        var extension = Path.GetExtension(request.FileName);
        if (BlockedExtensions.Contains(extension))
            return Result.Failure<AttachmentDto>($"File extension '{extension}' is not allowed.");

        // Validate file size
        var isVideo = VideoContentTypes.Contains(request.ContentType);
        var maxSize = isVideo ? MaxVideoFileSizeBytes : MaxFileSizeBytes;
        if (request.FileSize > maxSize)
        {
            var limitMb = maxSize / (1024 * 1024);
            return Result.Failure<AttachmentDto>($"File size exceeds the maximum allowed size of {limitMb} MB.");
        }

        // Parse entity type
        if (!Enum.TryParse<AttachmentEntityType>(request.EntityType, ignoreCase: true, out var entityType))
            return Result.Failure<AttachmentDto>($"Unknown entity type: '{request.EntityType}'.");

        // Create attachment entity to get Id before upload (needed for FileUrl)
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

        // Upload to storage
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
