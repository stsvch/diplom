using EduPlatform.Shared.Domain;
using MediatR;

namespace Notifications.Application.Notifications.Commands.DeleteNotification;

public record DeleteNotificationCommand(Guid Id, string UserId) : IRequest<Result>;
