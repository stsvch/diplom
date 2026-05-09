using MediatR;
using Messaging.Application.Interfaces;

namespace Messaging.Application.Queries.GetUnreadCount;

public record GetUnreadCountQuery(string UserId) : IRequest<int>;

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
