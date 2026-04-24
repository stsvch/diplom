using Auth.Domain.Entities;
using Auth.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Commands.Admin.ChangeUserRole;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, Result<string>>
{
    private static readonly string[] AllowedRoles = { "Admin", "Teacher", "Student" };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthDbContext _dbContext;

    public ChangeUserRoleCommandHandler(UserManager<ApplicationUser> userManager, IAuthDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<Result<string>> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (!AllowedRoles.Contains(request.NewRole))
            return Result.Failure<string>($"Недопустимая роль: {request.NewRole}.");

        if (request.UserId == request.ActorUserId)
            return Result.Failure<string>("Нельзя менять роль самому себе.");

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure<string>("Пользователь не найден.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        var isAdmin = currentRoles.Contains("Admin");
        if (isAdmin && !string.Equals(request.NewRole, "Admin", StringComparison.Ordinal))
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1)
                return Result.Failure<string>("Нельзя снять роль у последнего администратора платформы.");
        }

        if (currentRoles.Count == 1 && string.Equals(currentRoles[0], request.NewRole, StringComparison.Ordinal))
            return Result.Success($"Роль уже установлена: {request.NewRole}");

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return Result.Failure<string>(string.Join("; ", removeResult.Errors.Select(e => e.Description)));
        }

        var addResult = await _userManager.AddToRoleAsync(user, request.NewRole);
        if (!addResult.Succeeded)
            return Result.Failure<string>(string.Join("; ", addResult.Errors.Select(e => e.Description)));

        var stampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!stampResult.Succeeded)
            return Result.Failure<string>(string.Join("; ", stampResult.Errors.Select(e => e.Description)));

        var refreshTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == request.UserId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
            refreshToken.IsRevoked = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success($"Роль обновлена: {request.NewRole}");
    }
}
