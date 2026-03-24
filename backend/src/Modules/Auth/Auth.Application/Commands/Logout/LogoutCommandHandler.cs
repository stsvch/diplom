using Auth.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<string>>
{
    private readonly IAuthDbContext _dbContext;

    public LogoutCommandHandler(IAuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == request.UserId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Logged out successfully.");
    }
}
