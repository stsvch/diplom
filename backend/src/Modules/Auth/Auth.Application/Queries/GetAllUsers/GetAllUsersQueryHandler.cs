using Auth.Domain.Entities;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PagedResult<AdminUserDto>>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<PagedResult<AdminUserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        IEnumerable<ApplicationUser> source;
        if (!string.IsNullOrWhiteSpace(request.Role))
            source = await _userManager.GetUsersInRoleAsync(request.Role);
        else
            source = await _userManager.Users.AsNoTracking().ToListAsync(cancellationToken);

        var filtered = source.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var q = request.Search.Trim();
            filtered = filtered.Where(u =>
                (u.FirstName != null && u.FirstName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (u.LastName != null && u.LastName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (u.Email != null && u.Email.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        var now = DateTimeOffset.UtcNow;
        if (request.OnlyBlocked == true)
            filtered = filtered.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd.Value > now);

        var list = filtered.OrderByDescending(u => u.CreatedAt).ToList();
        var total = list.Count;
        var pageItems = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var dtos = new List<AdminUserDto>(pageItems.Count);
        foreach (var u in pageItems)
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

            dtos.Add(new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Role = role,
                IsBlocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > now,
                EmailConfirmed = u.EmailConfirmed,
                CreatedAt = u.CreatedAt
            });
        }

        return Result.Success(new PagedResult<AdminUserDto>(dtos, total, page, pageSize));
    }
}
