// DeleteChatCommand.cs
using EduPlatform.Shared.Domain;
using FluentValidation;
using MediatR;
using Messaging.Application.Interfaces;

namespace Messaging.Application.Commands.DeleteChat;

// Command содержит входные данные операции, изменяющей состояние модуля.
public record DeleteChatCommand(string ChatId) : IRequest<Result>;

// Валидатор проверяет параметры до выполнения handler-а.
public class DeleteChatValidator : AbstractValidator<DeleteChatCommand>
{
    public DeleteChatValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty();
    }
}

// Handler выполняет сценарий через контекст или репозитории и возвращает Result/DTO.
public class DeleteChatCommandHandler : IRequestHandler<DeleteChatCommand, Result>
{
    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;

    public DeleteChatCommandHandler(IMessagingRepository repository, IChatBroadcaster broadcaster)
    {
        _repository = repository;
        _broadcaster = broadcaster;
    }

    public async Task<Result> Handle(DeleteChatCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteChatAsync(request.ChatId);
        await _broadcaster.ChatDeletedAsync(request.ChatId);
        return Result.Success();
    }
}
