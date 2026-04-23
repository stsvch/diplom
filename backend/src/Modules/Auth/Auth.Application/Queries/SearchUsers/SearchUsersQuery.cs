using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.SearchUsers;

public record SearchUsersQuery(
    string? Query,
    string? Role,
    string? ExcludeUserId,
    int Limit = 20
) : IRequest<Result<List<UserSummaryDto>>>;
