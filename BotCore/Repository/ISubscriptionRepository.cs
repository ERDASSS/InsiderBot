using Contracts;

namespace BotCore.Repository;

public interface ISubscriptionRepository
{
    Task<List<SubscriptionType>> GetAllAsync(CancellationToken ct);
    Task<SubscriptionType?> FindByIdAsync(string id, CancellationToken ct);
    Task<List<SubscriptionType>> GetByChannelIdAsync(long channelId, CancellationToken ct);
}