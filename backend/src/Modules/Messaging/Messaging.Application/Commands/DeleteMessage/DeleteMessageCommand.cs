using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using FluentValidation;
using MediatR;
using Messaging.Application.Interfaces;

namespace Messaging.Application.Commands.DeleteMessage;

public record DeleteMessageCommand(string MessageId, string UserId) : IRequest<Result>;

public class DeleteMessageValidator : AbstractValidator<DeleteMessageCommand>
{
    public DeleteMessageValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, Result>
{
    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;
    private readonly IAttachmentCleaner _attachmentCleaner;

    public DeleteMessageCommandHandler(
        IMessagingRepository repository,
        IChatBroadcaster broadcaster,
        IAttachmentCleaner attachmentCleaner)
    {
        _repository = repository;
        _broadcaster = broadcaster;
        _attachmentCleaner = attachmentCleaner;
    }

    public async Task<Result> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _repository.GetMessageByIdAsync(request.MessageId);
        if (message == null)
            return Result.Failure("Сообщение не найдено.");

        var deleted = await _repository.DeleteMessageAsync(request.MessageId, request.UserId);
        if (!deleted)
            return Result.Failure("Нельзя удалить чужое сообщение.");

        var attachmentIds = message.Attachments
            .Where(a => a.AttachmentId.HasValue)
            .Select(a => a.AttachmentId!.Value)
            .ToList();
        if (attachmentIds.Count > 0)
            await _attachmentCleaner.DeleteAsync(attachmentIds, cancellationToken);

        await _broadcaster.MessageDeletedAsync(message.ChatId, request.MessageId);
        return Result.Success();
    }
}
