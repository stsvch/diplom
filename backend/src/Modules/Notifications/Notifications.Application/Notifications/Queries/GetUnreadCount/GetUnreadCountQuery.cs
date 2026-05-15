// Файл: GetUnreadCountQuery.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Notifications.Application.Notifications.Queries.GetUnreadCount;

// Запрос GetUnreadCountQuery описывает параметры чтения для MediatR-обработчика.
public record GetUnreadCountQuery(string UserId) : IRequest<Result<int>>;
