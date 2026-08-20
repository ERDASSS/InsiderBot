using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Contracts;

[BsonIgnoreExtraElements]
public class UserSubscription
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public long UserId { get; set; }
    
    public string SubscriptionTypeId { get; set; } = default!;
    
    public DateTime SubscribedAt { get; set; }
}