using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Contracts;

public class IncomingPost
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
 
    public long ChannelId { get; set; }
    public string Text { get; set; } = default!;
    public DateTime ReceivedAt { get; set; }
    
    public bool Processed { get; set; }
}
