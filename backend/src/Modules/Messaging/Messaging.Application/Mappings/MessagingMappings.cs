// MessagingMappings.cs
using Messaging.Application.DTOs;
using Messaging.Domain.Documents;

namespace Messaging.Application.Mappings;

// Маппинг централизует преобразование доменных или Mongo-моделей в DTO.
public static class MessagingMappings
{
    public static ChatDto ToChatDto(this ChatDocument chat, int unreadCount = 0) => new()
    {
        Id = chat.Id,
        Type = chat.Type.ToString(),
        CourseId = chat.CourseId,
        CourseName = chat.CourseName,
        OwnerId = chat.OwnerId,
        IsArchived = chat.IsArchived,
        Participants = chat.Participants.Select(p => new ParticipantDto
        {
            UserId = p.UserId,
            Name = p.Name
        }).ToList(),
        LastMessage = chat.LastMessage,
        LastMessageAt = chat.LastMessageAt,
        UnreadCount = unreadCount
    };

    public static MessageDto ToMessageDto(this MessageDocument msg) => new()
    {
        Id = msg.Id,
        ChatId = msg.ChatId,
        SenderId = msg.SenderId,
        SenderName = msg.SenderName,
        Text = msg.Text,
        Attachments = msg.Attachments.Select(a => new AttachmentDto
        {
            AttachmentId = a.AttachmentId,
            FileName = a.FileName,
            FileUrl = a.FileUrl,
            ContentType = a.ContentType,
            FileSize = a.FileSize
        }).ToList(),
        SentAt = msg.SentAt,
        IsEdited = msg.IsEdited
    };
}
