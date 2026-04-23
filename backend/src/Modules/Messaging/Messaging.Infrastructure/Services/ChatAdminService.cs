using EduPlatform.Shared.Application.Contracts;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Domain.Documents;

namespace Messaging.Infrastructure.Services;

public class ChatAdminService : IChatAdmin
{
    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;

    public ChatAdminService(IMessagingRepository repository, IChatBroadcaster broadcaster)
    {
        _repository = repository;
        _broadcaster = broadcaster;
    }

    public async Task CreateCourseChatAsync(
        string courseId,
        string courseName,
        string teacherId,
        string teacherName,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByCourseIdAsync(courseId);
        if (existing != null) return;

        await _repository.CreateCourseChatAsync(
            courseId,
            courseName,
            new List<string> { teacherId },
            new List<string> { teacherName },
            ownerId: teacherId);
    }

    public async Task AddParticipantAsync(
        string courseId,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var chat = await _repository.GetByCourseIdAsync(courseId);
        if (chat == null) return;

        var added = await _repository.AddParticipantAsync(chat.Id, userId, userName);
        if (!added) return;

        await _broadcaster.ParticipantAddedAsync(chat.Id, new ParticipantDto { UserId = userId, Name = userName });
        await _broadcaster.InstructUserJoinChatAsync(userId, chat.Id);
    }

    public async Task RemoveParticipantAsync(
        string courseId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var chat = await _repository.GetByCourseIdAsync(courseId);
        if (chat == null) return;

        var removed = await _repository.RemoveParticipantAsync(chat.Id, userId);
        if (!removed) return;

        await _broadcaster.RemoveUserFromChatAsync(userId, chat.Id);
        await _broadcaster.ParticipantRemovedAsync(chat.Id, userId);
    }

    public async Task DeleteCourseChatAsync(string courseId, CancellationToken cancellationToken = default)
    {
        var chat = await _repository.GetByCourseIdAsync(courseId);
        if (chat == null) return;

        await _repository.DeleteChatAsync(chat.Id);
        await _broadcaster.ChatDeletedAsync(chat.Id);
    }

    public async Task ArchiveCourseChatAsync(string courseId, CancellationToken cancellationToken = default)
    {
        var chat = await _repository.GetByCourseIdAsync(courseId);
        if (chat == null) return;

        await _repository.SetArchivedAsync(chat.Id, true);
        await _broadcaster.ChatArchivedAsync(chat.Id);
    }
}
