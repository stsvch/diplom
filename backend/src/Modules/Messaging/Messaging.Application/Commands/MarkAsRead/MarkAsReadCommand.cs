// MarkAsReadCommand.cs
using EduPlatform.Shared.Domain;
using FluentValidation;
using MediatR;
using Messaging.Application.Interfaces;

namespace Messaging.Application.Commands.MarkAsRead;

// Command содержит входные данные операции, изменяющей состояние модуля.
public record MarkAsReadCommand(string ChatId, string UserId) : IRequest<Result>;

// Валидатор проверяет параметры до выполнения handler-а.
public class MarkAsReadValidator : AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

// Handler выполняет сценарий через контекст или репозитории и возвращает Result/DTO.
public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;

    public MarkAsReadCommandHandler(IMessagingRepository repository, IChatBroadcaster broadcaster)
    {
        _repository = repository;
        _broadcaster = broadcaster;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        await _repository.MarkMessagesAsReadAsync(request.ChatId, request.UserId);
        await _broadcaster.MessagesReadAsync(request.ChatId, request.UserId);
        return Result.Success();
    }
}
