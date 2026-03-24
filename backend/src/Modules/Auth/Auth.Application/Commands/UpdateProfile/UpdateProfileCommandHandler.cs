using Auth.Application.DTOs;
using Auth.Domain.Entities;
using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure<UserProfileDto>("User not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure<UserProfileDto>(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var dto = _mapper.Map<UserProfileDto>(user);
        dto.Role = roles.FirstOrDefault() ?? string.Empty;

        return Result.Success(dto);
    }
}
