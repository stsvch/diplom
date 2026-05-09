using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.SearchUsers;

public record SearchUsersQuery(
    string? Query,
    string? Role,
    string? ExcludeUserId,
    int Limit = 20,
    Guid? RestrictToCourseId = null,
    string? RestrictToTeacherId = null
) : IRequest<Result<List<UserSummaryDto>>>;
