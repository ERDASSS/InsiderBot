using BotCore.Domain;
using Contracts;
using MongoDB.Driver;

namespace BotCore.Repository.Implementations;

public class MongoUserSubscriptionRepository(IMongoDatabase database) : IUserSubscriptionRepository
{
    private readonly IMongoCollection<UserSubscription> collection = database.GetCollection<UserSubscription>("user_subscriptions");

    public async Task SubscribeAsync(long userId, string subscriptionTypeId, CancellationToken ct)
    {
        var update = Builders<UserSubscription>.Update
            .SetOnInsert(x => x.UserId, userId)
            .Set(x => x.SubscriptionTypeId, subscriptionTypeId)
            .Set(x => x.SubscribedAt, DateTime.UtcNow);

        await collection.UpdateOneAsync(
            x => x.UserId == userId && x.SubscriptionTypeId == subscriptionTypeId,
            update,
            new UpdateOptions { IsUpsert = true },
            ct);
    }
 
    public Task UnsubscribeAsync(long userId, string subscriptionTypeId, CancellationToken ct) =>
        collection.DeleteOneAsync(x => x.UserId == userId && x.SubscriptionTypeId == subscriptionTypeId, ct);
 
    public Task<List<long>> GetSubscribedUserIdsAsync(string subscriptionTypeId, CancellationToken ct) =>
        collection.Find(x => x.SubscriptionTypeId == subscriptionTypeId)
            .Project(x => x.UserId)
            .ToListAsync(ct);

    public async Task<List<string>> GetUserSubscriptionTypeIdsAsync(
        long userId,
        CancellationToken ct)
    {
        var docs = await collection
            .Find(x => x.UserId == userId)
            .ToListAsync(ct);

        Console.WriteLine($"UserId: {userId}");
        Console.WriteLine($"Found: {docs.Count}");

        foreach (var doc in docs)
        {
            Console.WriteLine(
                $"Id={doc.Id}, UserId={doc.UserId}, SubscriptionTypeId={doc.SubscriptionTypeId}");
        }

        return docs
            .Select(x => x.SubscriptionTypeId)
            .ToList();
    }
}