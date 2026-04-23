using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Queries.SearchUsers;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, Result<List<UserSummaryDto>>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SearchUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<List<UserSummaryDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Max(1, Math.Min(request.Limit, 50));

        IEnumerable<ApplicationUser> source;
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            // Role filter applied at DB layer — returns users only in that role
            source = await _userManager.GetUsersInRoleAsync(request.Role);
        }
        else
        {
            source = await _userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
        }

        var filtered = source.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(request.ExcludeUserId))
            filtered = filtered.Where(u => u.Id != request.ExcludeUserId);

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
