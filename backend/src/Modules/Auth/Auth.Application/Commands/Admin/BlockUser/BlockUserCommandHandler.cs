using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Auth.Application.Interfaces;

namespace Auth.Application.Commands.Admin.BlockUser;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthDbContext _dbContext;

    public BlockUserCommandHandler(UserManager<ApplicationUser> userManager, IAuthDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<Result<string>> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure<string>("Пользователь не найден.");

        if (!user.LockoutEnabled)
        {
            var enableResult = await _userManager.SetLockoutEnabledAsync(user, true);
            if (!enableResult.Succeeded)
                return Result.Failure<string>(string.Join("; ", enableResult.Errors.Select(e => e.Description)));
        }

        var setResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (!setResult.Succeeded)
            return Result.Failure<string>(string.Join("; ", setResult.Errors.Select(e => e.Description)));

        var refreshTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == request.UserId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
            refreshToken.IsRevoked = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Пользователь заблокирован.");
    }
}
