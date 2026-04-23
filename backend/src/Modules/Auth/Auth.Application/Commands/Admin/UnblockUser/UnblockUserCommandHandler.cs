using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Commands.Admin.UnblockUser;

public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UnblockUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<string>> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure<string>("Пользователь не найден.");

        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        if (!result.Succeeded)
            return Result.Failure<string>(string.Join("; ", result.Errors.Select(e => e.Description)));

        return Result.Success("Пользователь разблокирован.");
    }
}
