// GetUserChatsQuery.cs
using MediatR;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Application.Mappings;

namespace Messaging.Application.Queries.GetUserChats;

// Query описывает параметры чтения без изменения состояния.
public record GetUserChatsQuery(string UserId) : IRequest<List<ChatDto>>;

// Handler собирает данные для чтения и маппит их в DTO.
public class GetUserChatsQueryHandler : IRequestHandler<GetUserChatsQuery, List<ChatDto>>
{
    private readonly IMessagingRepository _repository;

    public GetUserChatsQueryHandler(IMessagingRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ChatDto>> Handle(GetUserChatsQuery request, CancellationToken cancellationToken)
    {
        var chats = await _repository.GetUserChatsAsync(request.UserId);
        var unread = await _repository.GetUnreadCountsPerChatAsync(request.UserId);
        return chats.Select(c => c.ToChatDto(unread.GetValueOrDefault(c.Id, 0))).ToList();
    }
}
