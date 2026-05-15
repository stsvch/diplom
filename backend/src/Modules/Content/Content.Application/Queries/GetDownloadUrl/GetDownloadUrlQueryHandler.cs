// GetDownloadUrlQueryHandler.cs

using Content.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Queries.GetDownloadUrl;

// Тип class: ключевой элемент файла GetDownloadUrlQueryHandler.cs.
public class GetDownloadUrlQueryHandler : IRequestHandler<GetDownloadUrlQuery, Result<string>>
{
    private readonly IContentDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public GetDownloadUrlQueryHandler(IContentDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<string>> Handle(GetDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _context.Attachments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (attachment == null)
            return Result.Failure<string>("File not found.");

        var presignedUrl = await _fileStorageService.GetDownloadUrlAsync(attachment.StoragePath, cancellationToken);

        return Result.Success(presignedUrl);
    }
}
