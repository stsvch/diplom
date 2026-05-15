// GetAllUsersQuery.cs
using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.GetAllUsers;

// Запрос для админской таблицы пользователей; сейчас не принимает фильтры, поэтому отдаёт весь список.
public record GetAllUsersQuery(
    string? Search,
    string? Role,
    bool? OnlyBlocked,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<AdminUserDto>>>;
