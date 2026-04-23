using Auth.Domain.Entities;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Commands.Admin.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserDeletionGuard _userDeletionGuard;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager, IUserDeletionGuard userDeletionGuard)
    {
        _userManager = userManager;
        _userDeletionGuard = userDeletionGuard;
    }

    public async Task<Result<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure<string>("Пользователь не найден.");

        var check = await _userDeletionGuard.CheckAsync(request.UserId, cancellationToken);
        if (!check.CanDelete)
        {
            var reasons = string.Join("; ", check.BlockingReasons);
            return Result.Failure<string>(
                $"Пользователя нельзя удалить физически: {reasons}. Используйте блокировку вместо удаления.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return Result.Failure<string>(string.Join("; ", result.Errors.Select(e => e.Description)));

        return Result.Success("Пользователь удалён.");
    }
}
