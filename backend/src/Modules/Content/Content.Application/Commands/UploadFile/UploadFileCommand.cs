using Content.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Commands.UploadFile;

public class UploadFileCommand : IRequest<Result<AttachmentDto>>
{
    public Stream Stream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string UploadedById { get; set; } = string.Empty;
}
