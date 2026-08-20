namespace BotCore.Domain;

public interface ISubscriptionKeywordMatcher
{
    Task<bool> IsMatchAsync(
        string subscriptionTypeId,
        string text,
        CancellationToken ct);
}