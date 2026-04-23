using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.GetAllUsers;

public record GetAllUsersQuery(
    string? Search,
    string? Role,
    bool? OnlyBlocked,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<AdminUserDto>>>;
