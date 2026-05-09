using MediatR;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Application.Mappings;

namespace Messaging.Application.Queries.GetChatMessages;

public record GetChatMessagesQuery(string ChatId, int Page, int PageSize) : IRequest<List<MessageDto>>;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, List<MessageDto>>
{
    private readonly IMessagingRepository _repository;

    public GetChatMessagesQueryHandler(IMessagingRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var messages = await _repository.GetChatMessagesAsync(request.ChatId, page, pageSize);
        return messages.Select(m => m.ToMessageDto()).ToList();
    }
}
