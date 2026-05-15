// GetProfileQuery.cs
using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.GetProfile;

// Запрос профиля по id текущего пользователя из JWT.
public record GetProfileQuery(string UserId) : IRequest<Result<UserProfileDto>>;
