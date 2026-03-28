using EduPlatform.Shared.Domain;
using MediatR;

namespace Notifications.Application.Notifications.Queries.GetUnreadCount;

public record GetUnreadCountQuery(string UserId) : IRequest<Result<int>>;
