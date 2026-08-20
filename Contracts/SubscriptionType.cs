using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Contracts;

[BsonIgnoreExtraElements]
public class SubscriptionType
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;      
    
    public string Name { get; set; } = default!;    
    
    [BsonElement("ChannelIds")]
    public List<long> ChannelIds { get; set; } = [];
}