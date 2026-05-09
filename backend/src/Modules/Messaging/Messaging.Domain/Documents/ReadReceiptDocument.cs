using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Messaging.Domain.Documents;

public class ReadReceiptDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string ChatId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime LastReadAt { get; set; }
}
