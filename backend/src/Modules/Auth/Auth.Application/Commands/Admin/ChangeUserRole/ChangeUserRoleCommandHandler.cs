using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Commands.Admin.ChangeUserRole;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, Result<string>>
{
    private static readonly string[] AllowedRoles = { "Admin", "Teacher", "Student" };

    private readonly UserManager<ApplicationUser> _userManager;

    public ChangeUserRoleCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<string>> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (!AllowedRoles.Contains(request.NewRole))
            return Result.Failure<string>($"Недопустимая роль: {request.NewRole}.");

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure<string>("Пользователь не найден.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return Result.Failure<string>(string.Join("; ", removeResult.Errors.Select(e => e.Description)));
        }

        var addResult = await _userManager.AddToRoleAsync(user, request.NewRole);
        if (!addResult.Succeeded)
            return Result.Failure<string>(string.Join("; ", addResult.Errors.Select(e => e.Description)));

        return Result.Success($"Роль обновлена: {request.NewRole}");
    }
}
