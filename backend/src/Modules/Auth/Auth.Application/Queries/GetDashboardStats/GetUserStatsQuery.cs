// GetUserStatsQuery.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.GetDashboardStats;

// Запрос агрегатов по пользователям: всего, по ролям, заблокированные, неподтверждённые и новые за неделю.
public record GetUserStatsQuery() : IRequest<Result<UserStatsDto>>;
