using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.GetProfile;

public record GetProfileQuery(string UserId) : IRequest<Result<UserProfileDto>>;
