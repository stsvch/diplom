using Content.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Commands.DeleteFile;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result<string>>
{
    private readonly IContentDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public DeleteFileCommandHandler(IContentDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<string>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _context.Attachments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (attachment == null)
            return Result.Failure<string>("File not found.");

        // Only the uploader or admin can delete
        var isAdmin = false; // Admin check is done via roles in the controller layer
        if (attachment.UploadedById != request.UserId && !isAdmin)
            return Result.Failure<string>("You do not have permission to delete this file.");

        await _fileStorageService.DeleteAsync(attachment.StoragePath, cancellationToken);

        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("File deleted successfully.");
    }
}
