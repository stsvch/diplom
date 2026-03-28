using EduPlatform.Shared.Domain;
using MediatR;

namespace Notifications.Application.Notifications.Commands.MarkAllAsRead;

public record MarkAllAsReadCommand(string UserId) : IRequest<Result>;
