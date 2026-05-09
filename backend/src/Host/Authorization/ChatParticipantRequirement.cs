using Microsoft.AspNetCore.Authorization;

namespace EduPlatform.Host.Authorization;

public sealed class ChatParticipantRequirement : IAuthorizationRequirement;

public sealed class CourseChatOwnerRequirement : IAuthorizationRequirement;
