using EduPlatform.Shared.Domain;
using FluentValidation;
using MediatR;
using Messaging.Application.Interfaces;

namespace Messaging.Application.Commands.DeleteChat;

public record DeleteChatCommand(string ChatId) : IRequest<Result>;

public class DeleteChatValidator : AbstractValidator<DeleteChatCommand>
{
    public DeleteChatValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty();
    }
}

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
