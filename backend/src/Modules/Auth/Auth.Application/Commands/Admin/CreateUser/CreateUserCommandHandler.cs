using Auth.Application.Queries.SearchUsers;
using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Commands.Admin.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserSummaryDto>>
{
    private static readonly string[] AllowedRoles = { "Admin", "Teacher", "Student" };

    private readonly UserManager<ApplicationUser> _userManager;

    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserSummaryDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!AllowedRoles.Contains(request.Role))
            return Result.Failure<UserSummaryDto>($"Недопустимая роль: {request.Role}.");

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return Result.Failure<UserSummaryDto>("Пользователь с таким email уже существует.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true, // admin-created accounts skip email confirmation
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Result.Failure<UserSummaryDto>(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
            return Result.Failure<UserSummaryDto>(string.Join("; ", roleResult.Errors.Select(e => e.Description)));

        return Result.Success(new UserSummaryDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email!,
            Role = request.Role
        });
    }
}
