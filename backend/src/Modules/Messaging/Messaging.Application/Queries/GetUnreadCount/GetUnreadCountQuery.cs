// GetUnreadCountQuery.cs
using MediatR;
using Messaging.Application.Interfaces;

namespace Messaging.Application.Queries.GetUnreadCount;

// Query описывает параметры чтения без изменения состояния.
public record GetUnreadCountQuery(string UserId) : IRequest<int>;

// Handler собирает данные для чтения и маппит их в DTO.
public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly IMessagingRepository _repository;

    public GetUnreadCountQueryHandler(IMessagingRepository repository)
    {
        _repository = repository;
    }

    public Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken) =>
        _repository.GetUnreadCountAsync(request.UserId);
}
