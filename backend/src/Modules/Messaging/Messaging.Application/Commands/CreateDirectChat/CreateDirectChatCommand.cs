using EduPlatform.Shared.Domain;
using FluentValidation;
using MediatR;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Application.Mappings;

namespace Messaging.Application.Commands.CreateDirectChat;

public record CreateDirectChatCommand(
    string InitiatorId,
    string InitiatorName,
    string RecipientId,
    string RecipientName
) : IRequest<Result<ChatDto>>;

public class CreateDirectChatValidator : AbstractValidator<CreateDirectChatCommand>
{
    public CreateDirectChatValidator()
    {
        RuleFor(x => x.InitiatorId).NotEmpty();
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x).Must(c => c.InitiatorId != c.RecipientId)
            .WithMessage("Нельзя создать чат с самим собой.");
    }
}

public class CreateDirectChatCommandHandler : IRequestHandler<CreateDirectChatCommand, Result<ChatDto>>
{
    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;

    public CreateDirectChatCommandHandler(IMessagingRepository repository, IChatBroadcaster broadcaster)
    {
        _repository = repository;
        _broadcaster = broadcaster;
    }

    public async Task<Result<ChatDto>> Handle(CreateDirectChatCommand request, CancellationToken cancellationToken)
    {
        var chat = await _repository.GetOrCreateDirectChatAsync(
            request.InitiatorId, request.InitiatorName,
            request.RecipientId, request.RecipientName);

        await _broadcaster.InstructUserJoinChatAsync(request.InitiatorId, chat.Id);
        await _broadcaster.InstructUserJoinChatAsync(request.RecipientId, chat.Id);

        return Result.Success(chat.ToChatDto());
    }
}
