using BotCore.Repository;
using Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotCore.Domain.Implementations;

public class ShowCommandHandler(
    ISubscriptionRepository subscriptionsRepository,
    IUserSubscriptionRepository userSubscriptionsRepository) : ICommandHandler
{
    public string Command => CommandsConsts.Show;

    public async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        var userId = message.From!.Id;
        
        var subscribedTypeIds = await userSubscriptionsRepository.GetUserSubscriptionTypeIdsAsync(userId, ct);

        if (subscribedTypeIds.Count == 0)
        {
            await bot.SendMessage(
                userId,
                "У вас пока нет активных подписок.\nИспользуйте /subscribe, чтобы оформить подписку.",
                cancellationToken: ct);
            return;
        }
        
        var allSubscriptionTypes = await subscriptionsRepository.GetAllAsync(ct);
        
        var mySubscriptions = allSubscriptionTypes
            .Where(st => subscribedTypeIds.Contains(st.Id))
            .ToList();

        if (mySubscriptions.Count == 0)
        {
            await bot.SendMessage(
                userId,
                "Ваши предыдущие подписки были удалены администратором.",
                cancellationToken: ct);
            return;
        }
        
        var list = string.Join("\n", mySubscriptions.Select(s => $"✅ {s.Name} (ID: {s.Id})"));

        await bot.SendMessage(
            userId,
            $"Ваши активные подписки:\n\n{list}\n\nЧтобы отписаться, используйте команду:\n/unsubscribe <id>",
            cancellationToken: ct);
    }
}