// SearchUsersQueryHandler.cs
using Auth.Domain.Entities;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Queries.SearchUsers;

// Обработчик сценария: ищет пользователей по строке и роли, возвращая компактные карточки для выбора пользователя.
public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, Result<List<UserSummaryDto>>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEnrollmentReadService _enrollmentReader;

    public SearchUsersQueryHandler(
        UserManager<ApplicationUser> userManager,
        IEnrollmentReadService enrollmentReader)
    {
        _userManager = userManager;
        _enrollmentReader = enrollmentReader;
    }

    public async Task<Result<List<UserSummaryDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Max(1, Math.Min(request.Limit, 50));

        IEnumerable<ApplicationUser> source;
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            // Фильтр по роли применяется на уровне БД и возвращает только пользователей этой роли.
            source = await _userManager.GetUsersInRoleAsync(request.Role);
        }
        else
        {
            source = await _userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
        }

        var filtered = source.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(request.ExcludeUserId))
            filtered = filtered.Where(u => u.Id != request.ExcludeUserId);

        if (request.RestrictToCourseId.HasValue)
        {
            var enrolled = await _enrollmentReader.GetActiveStudentIdsAsync(request.RestrictToCourseId.Value, cancellationToken);
            var allowed = enrolled.ToHashSet();
            filtered = filtered.Where(u => allowed.Contains(u.Id));
        }
        else if (!string.IsNullOrWhiteSpace(request.RestrictToTeacherId))
        {
            var connected = await _enrollmentReader.GetActiveStudentIdsForTeacherAsync(request.RestrictToTeacherId, cancellationToken);
            var allowed = connected.ToHashSet();
            filtered = filtered.Where(u => allowed.Contains(u.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var q = request.Query.Trim();
            filtered = filtered.Where(u =>
                (u.FirstName != null && u.FirstName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (u.LastName != null && u.LastName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (u.Email != null && u.Email.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        var page = filtered.Take(limit).ToList();

        var result = new List<UserSummaryDto>(page.Count);
        foreach (var u in page)
        {
            string role;
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                role = request.Role;
            }
            else
            {
                var roles = await _userManager.GetRolesAsync(u);
                role = roles.FirstOrDefault() ?? string.Empty;
            }

            result.Add(new UserSummaryDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email ?? string.Empty,
                Role = role
            });
        }

        return Result.Success(result);
    }
}
