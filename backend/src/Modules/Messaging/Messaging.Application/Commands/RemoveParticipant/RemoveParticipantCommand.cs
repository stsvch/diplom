// RemoveParticipantCommand.cs
using EduPlatform.Shared.Domain;
using FluentValidation;
using MediatR;
using Messaging.Application.Interfaces;

namespace Messaging.Application.Commands.RemoveParticipant;

// Command содержит входные данные операции, изменяющей состояние модуля.
public record RemoveParticipantCommand(string ChatId, string ParticipantId) : IRequest<Result>;

// Валидатор проверяет параметры до выполнения handler-а.
public class RemoveParticipantValidator : AbstractValidator<RemoveParticipantCommand>
{
    public RemoveParticipantValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty();
        RuleFor(x => x.ParticipantId).NotEmpty();
    }
}

// Handler выполняет сценарий через контекст или репозитории и возвращает Result/DTO.
public class RemoveParticipantCommandHandler : IRequestHandler<RemoveParticipantCommand, Result>
{
    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;

    public RemoveParticipantCommandHandler(IMessagingRepository repository, IChatBroadcaster broadcaster)
    {
        _repository = repository;
        _broadcaster = broadcaster;
    }

    public async Task<Result> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken)
    {
        var removed = await _repository.RemoveParticipantAsync(request.ChatId, request.ParticipantId);
        if (!removed)
            return Result.Failure("Участник не найден.");

        await _broadcaster.RemoveUserFromChatAsync(request.ParticipantId, request.ChatId);
        await _broadcaster.ParticipantRemovedAsync(request.ChatId, request.ParticipantId);
        return Result.Success();
    }
}
