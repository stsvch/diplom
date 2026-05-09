using Messaging.Application.Interfaces;
using Messaging.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EduPlatform.Host.Authorization;

public class ChatParticipantAuthorizationHandler : AuthorizationHandler<ChatParticipantRequirement>
{
    private readonly IMessagingRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChatParticipantAuthorizationHandler(
        IMessagingRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ChatParticipantRequirement requirement)
    {
        var chatId = _httpContextAccessor.HttpContext?.Request.RouteValues["chatId"]?.ToString();
        if (string.IsNullOrEmpty(chatId)) return;

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat != null && chat.ParticipantIds.Contains(userId))
            context.Succeed(requirement);
    }
}

public class CourseChatOwnerAuthorizationHandler : AuthorizationHandler<CourseChatOwnerRequirement>
{
    private readonly IMessagingRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CourseChatOwnerAuthorizationHandler(
        IMessagingRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseChatOwnerRequirement requirement)
    {
        var chatId = _httpContextAccessor.HttpContext?.Request.RouteValues["chatId"]?.ToString();
        if (string.IsNullOrEmpty(chatId)) return;

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        var chat = await _repository.GetChatByIdAsync(chatId);
        if (chat != null && chat.Type == ChatType.CourseChat && chat.OwnerId == userId)
            context.Succeed(requirement);
    }
}
