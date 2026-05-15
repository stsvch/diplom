// SearchUsersQuery.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.SearchUsers;

// Запрос поиска пользователей хранит поисковую строку, необязательный фильтр роли и лимит результата.
public record SearchUsersQuery(
    string? Query,
    string? Role,
    string? ExcludeUserId,
    int Limit = 20,
    Guid? RestrictToCourseId = null,
    string? RestrictToTeacherId = null
) : IRequest<Result<List<UserSummaryDto>>>;
