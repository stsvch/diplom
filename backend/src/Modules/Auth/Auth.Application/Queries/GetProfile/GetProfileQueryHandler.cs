using Auth.Application.DTOs;
using Auth.Domain.Entities;
using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetProfileQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure<UserProfileDto>("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var dto = _mapper.Map<UserProfileDto>(user);
        dto.Role = roles.FirstOrDefault() ?? string.Empty;

        return Result.Success(dto);
    }
}
