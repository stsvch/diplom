using Messaging.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Messaging.Domain.Documents;

[BsonIgnoreExtraElements]
public class ChatDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.String)]
    public ChatType Type { get; set; } = ChatType.DirectMessage;
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public List<string> ParticipantIds { get; set; } = new();
    public List<ParticipantInfo> Participants { get; set; } = new();
    public string? OwnerId { get; set; }
    public bool IsArchived { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ParticipantInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
