// GetChatByIdQuery.cs
using EduPlatform.Shared.Domain;
using MediatR;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Application.Mappings;

namespace Messaging.Application.Queries.GetChatById;

// Query описывает параметры чтения без изменения состояния.
public record GetChatByIdQuery(string ChatId, string UserId) : IRequest<Result<ChatDto>>;

// Handler собирает данные для чтения и маппит их в DTO.
public class GetChatByIdQueryHandler : IRequestHandler<GetChatByIdQuery, Result<ChatDto>>
{
    private readonly IMessagingRepository _repository;

    public GetChatByIdQueryHandler(IMessagingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ChatDto>> Handle(GetChatByIdQuery request, CancellationToken cancellationToken)
    {
        var chat = await _repository.GetChatByIdAsync(request.ChatId);
        if (chat == null)
            return Result.Failure<ChatDto>("Чат не найден.");

        var unread = await _repository.GetUnreadCountsPerChatAsync(request.UserId);
        return Result.Success(chat.ToChatDto(unread.GetValueOrDefault(request.ChatId, 0)));
    }
}
