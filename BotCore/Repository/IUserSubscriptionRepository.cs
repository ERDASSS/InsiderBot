using Contracts;

namespace BotCore.Repository;

public interface IUserSubscriptionRepository
{
    Task SubscribeAsync(long userId, string subscriptionTypeId, CancellationToken ct);
    Task UnsubscribeAsync(long userId, string subscriptionTypeId, CancellationToken ct);
    Task<List<long>> GetSubscribedUserIdsAsync(string subscriptionTypeId, CancellationToken ct);
    Task<List<string>> GetUserSubscriptionTypeIdsAsync(long userId, CancellationToken ct);
}