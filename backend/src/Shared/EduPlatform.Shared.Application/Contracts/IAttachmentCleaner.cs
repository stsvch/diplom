namespace EduPlatform.Shared.Application.Contracts;

public interface IAttachmentCleaner
{
    Task DeleteAsync(IEnumerable<Guid> attachmentIds, CancellationToken cancellationToken = default);
}
