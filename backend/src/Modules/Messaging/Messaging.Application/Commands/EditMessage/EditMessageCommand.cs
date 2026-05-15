// EditMessageCommand.cs
using EduPlatform.Shared.Domain;
using FluentValidation;
using MediatR;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Application.Mappings;

namespace Messaging.Application.Commands.EditMessage;

// Command содержит входные данные операции, изменяющей состояние модуля.
public record EditMessageCommand(string MessageId, string UserId, string Text)
    : IRequest<Result<MessageDto>>;

// Валидатор проверяет параметры до выполнения handler-а.
public class EditMessageValidator : AbstractValidator<EditMessageCommand>
{
    public EditMessageValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4000);
    }
}

// Handler выполняет сценарий через контекст или репозитории и возвращает Result/DTO.
public class EditMessageCommandHandler : IRequestHandler<EditMessageCommand, Result<MessageDto>>
{
    private static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(15);

    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;

    public EditMessageCommandHandler(IMessagingRepository repository, IChatBroadcaster broadcaster)
    {
        _repository = repository;
        _broadcaster = broadcaster;
    }

    public async Task<Result<MessageDto>> Handle(EditMessageCommand request, CancellationToken cancellationToken)
    {
        var edited = await _repository.EditMessageAsync(request.MessageId, request.UserId, request.Text, EditWindow);
        if (!edited)
            return Result.Failure<MessageDto>("Сообщение не найдено, не ваше или истёк срок редактирования (15 мин).");

        var updated = await _repository.GetMessageByIdAsync(request.MessageId);
        if (updated == null)
            return Result.Failure<MessageDto>("Сообщение не найдено.");

        var dto = updated.ToMessageDto();
        await _broadcaster.MessageEditedAsync(updated.ChatId, dto);
        return Result.Success(dto);
    }
}
