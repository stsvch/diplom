using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Messaging.Domain.Documents;

public class MessageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string ChatId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<MessageAttachment> Attachments { get; set; } = new();
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public List<string> ReadBy { get; set; } = new();
    public bool IsEdited { get; set; }
}

public class MessageAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
