using EduPlatform.Shared.Domain;
using MediatR;

namespace Notifications.Application.Notifications.Commands.MarkAsRead;

public record MarkAsReadCommand(Guid Id, string UserId) : IRequest<Result>;
