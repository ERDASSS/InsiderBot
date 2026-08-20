using BotCore.Repository;
using Contracts;
using Telegram.Bot;

namespace BotCore.Domain.Implementations;

public class ShowCommandHandler(
    ISubscriptionRepository subscriptionsRepository,
    IUserSubscriptionRepository userSubscriptionsRepository) : ICommandHandler
{
    public string Command => CommandsConsts.Show;

    public async Task HandleAsync(ITelegramBotClient bot, CommandContext context, CancellationToken ct)
    {
        var subscribedTypeIds = await userSubscriptionsRepository.GetUserSubscriptionTypeIdsAsync(context.UserId, ct);

        if (subscribedTypeIds.Count == 0)
        {
            await bot.SendMessage(
                context.ChatId,
                "У вас пока нет активных подписок.\nИспользуйте /subscribe, чтобы оформить подписку.",
                replyMarkup: BotKeyboards.MainMenu,
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
                context.ChatId,
                "Ваши предыдущие подписки были удалены администратором.",
                replyMarkup: BotKeyboards.MainMenu,
                cancellationToken: ct);
            return;
        }

        var list = string.Join("\n", mySubscriptions.Select(s => $"✅ {s.Name} (ID: {s.Id})"));

        await bot.SendMessage(
            context.ChatId,
            $"Ваши активные подписки:\n\n{list}",
            replyMarkup: BotKeyboards.MainMenu,
            cancellationToken: ct);
    }
}
