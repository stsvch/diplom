// SendMessageCommand.cs
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using FluentValidation;
using MediatR;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Application.Mappings;
using Messaging.Domain.Documents;

namespace Messaging.Application.Commands.SendMessage;

// Command содержит входные данные операции, изменяющей состояние модуля.
public record SendMessageCommand(
    string ChatId,
    string SenderId,
    string SenderName,
    string Text,
    IReadOnlyList<AttachmentDto>? Attachments
) : IRequest<Result<MessageDto>>;

// Валидатор проверяет параметры до выполнения handler-а.
public class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty();
        RuleFor(x => x.SenderId).NotEmpty();
        RuleFor(x => x).Must(c =>
            !string.IsNullOrWhiteSpace(c.Text) || (c.Attachments is { Count: > 0 }))
            .WithMessage("Сообщение не может быть пустым.");
        RuleFor(x => x.Text!).MaximumLength(4000).When(x => x.Text != null);
    }
}

// Handler выполняет сценарий через контекст или репозитории и возвращает Result/DTO.
public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;
    private readonly INotificationDispatcher _notifications;

    public SendMessageCommandHandler(
        IMessagingRepository repository,
        IChatBroadcaster broadcaster,
        INotificationDispatcher notifications)
    {
        _repository = repository;
        _broadcaster = broadcaster;
        _notifications = notifications;
    }

    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var chat = await _repository.GetChatByIdAsync(request.ChatId);
        if (chat == null)
            return Result.Failure<MessageDto>("Чат не найден.");
        if (chat.IsArchived)
            return Result.Failure<MessageDto>("Чат архивирован.");

        var doc = new MessageDocument
        {
            ChatId = request.ChatId,
            SenderId = request.SenderId,
            SenderName = request.SenderName,
            Text = request.Text ?? string.Empty,
            Attachments = request.Attachments?.Select(a => new MessageAttachment
            {
                AttachmentId = a.AttachmentId,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList() ?? new List<MessageAttachment>(),
            SentAt = DateTime.UtcNow
        };

        var saved = await _repository.SendMessageAsync(doc);
        await _repository.MarkMessagesAsReadAsync(request.ChatId, request.SenderId);
        var dto = saved.ToMessageDto();

        await _broadcaster.MessageSentAsync(request.ChatId, dto, chat.ParticipantIds);

        var recipients = chat.ParticipantIds.Where(id => id != request.SenderId).ToList();
        if (recipients.Count > 0)
        {
            var safeText = request.Text ?? string.Empty;
            var preview = safeText.Length > 80 ? safeText.Substring(0, 80) + "…" : safeText;
            if (string.IsNullOrEmpty(preview) && saved.Attachments.Count > 0)
                preview = $"[вложение: {saved.Attachments.Count}]";

            var notifications = recipients.Select(rid => new NotificationRequest(
                rid, NotificationType.Message, "Новое сообщение",
                $"{request.SenderName}: {preview}", $"/messages/{request.ChatId}")).ToList();
            await _notifications.PublishManyAsync(notifications);
        }

        return Result.Success(dto);
    }
}
