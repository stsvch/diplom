// Файл: ChatParticipantRequirement.cs
using Microsoft.AspNetCore.Authorization;

namespace EduPlatform.Host.Authorization;

// Требование авторизации ChatParticipantRequirement используется политикой доступа в Host.
public sealed class ChatParticipantRequirement : IAuthorizationRequirement;

// Требование авторизации CourseChatOwnerRequirement используется политикой доступа в Host.
public sealed class CourseChatOwnerRequirement : IAuthorizationRequirement;
