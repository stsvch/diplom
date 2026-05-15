// UpdateProfileCommand.cs
using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.UpdateProfile;

// Команда хранит id пользователя и редактируемые поля профиля: имя, фамилию и ссылку на аватар.
public record UpdateProfileCommand(
    string UserId,
    string FirstName,
    string LastName,
    string? AvatarUrl
) : IRequest<Result<UserProfileDto>>;
