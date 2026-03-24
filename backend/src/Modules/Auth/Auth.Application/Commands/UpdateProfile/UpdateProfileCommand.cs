using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string UserId,
    string FirstName,
    string LastName,
    string? AvatarUrl
) : IRequest<Result<UserProfileDto>>;
