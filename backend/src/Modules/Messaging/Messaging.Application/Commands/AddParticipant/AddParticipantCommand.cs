// AddParticipantCommand.cs
using EduPlatform.Shared.Domain;
using FluentValidation;
using MediatR;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;

namespace Messaging.Application.Commands.AddParticipant;

// Command содержит входные данные операции, изменяющей состояние модуля.
public record AddParticipantCommand(string ChatId, string UserId, string UserName)
    : IRequest<Result<bool>>;

// Валидатор проверяет параметры до выполнения handler-а.
public class AddParticipantValidator : AbstractValidator<AddParticipantCommand>
{
    public AddParticipantValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UserName).NotEmpty();
    }
}

// Handler выполняет сценарий через контекст или репозитории и возвращает Result/DTO.
public class AddParticipantCommandHandler : IRequestHandler<AddParticipantCommand, Result<bool>>
{
    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;

    public AddParticipantCommandHandler(IMessagingRepository repository, IChatBroadcaster broadcaster)
    {
        _repository = repository;
        _broadcaster = broadcaster;
    }

    public async Task<Result<bool>> Handle(AddParticipantCommand request, CancellationToken cancellationToken)
    {
        var added = await _repository.AddParticipantAsync(request.ChatId, request.UserId, request.UserName);
        if (!added)
            return Result.Success(false);

        await _broadcaster.ParticipantAddedAsync(request.ChatId,
            new ParticipantDto { UserId = request.UserId, Name = request.UserName });
        await _broadcaster.InstructUserJoinChatAsync(request.UserId, request.ChatId);
        return Result.Success(true);
    }
}
