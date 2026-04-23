using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.GetDashboardStats;

public record GetUserStatsQuery() : IRequest<Result<UserStatsDto>>;
