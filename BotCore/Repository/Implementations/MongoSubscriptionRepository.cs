using BotCore.Domain;
using Contracts;
using MongoDB.Driver;

namespace BotCore.Repository.Implementations;

public class MongoSubscriptionRepository(IMongoDatabase database) : ISubscriptionRepository
{
    private readonly IMongoCollection<SubscriptionType> collection = database.GetCollection<SubscriptionType>("subscription_types");

    public Task<List<SubscriptionType>> GetAllAsync(CancellationToken ct) =>
        collection.Find(FilterDefinition<SubscriptionType>.Empty).ToListAsync(ct);
 
    public Task<SubscriptionType?> FindByIdAsync(string id, CancellationToken ct) =>
        collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct)!;
 
    public Task<List<SubscriptionType>> GetByChannelIdAsync(long channelId, CancellationToken ct) =>
        collection.Find(x => x.ChannelIds.Contains(channelId)).ToListAsync(ct);
}