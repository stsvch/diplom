using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Queries.GetDashboardStats;

public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, Result<UserStatsDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserStatsQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserStatsDto>> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var total = await _userManager.Users.CountAsync(cancellationToken);
        var blocked = await _userManager.Users
            .CountAsync(u => u.LockoutEnd.HasValue && u.LockoutEnd.Value > now, cancellationToken);
        var unconfirmed = await _userManager.Users
            .CountAsync(u => !u.EmailConfirmed, cancellationToken);
        var newLast7 = await _userManager.Users
            .CountAsync(u => u.CreatedAt >= weekAgo, cancellationToken);

        var students = (await _userManager.GetUsersInRoleAsync("Student")).Count;
        var teachers = (await _userManager.GetUsersInRoleAsync("Teacher")).Count;
        var admins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;

        return Result.Success(new UserStatsDto
        {
            Total = total,
            Students = students,
            Teachers = teachers,
            Admins = admins,
            Blocked = blocked,
            UnconfirmedEmail = unconfirmed,
            NewLast7Days = newLast7,
        });
    }
}
