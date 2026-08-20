using BotCore.Repository;
using Contracts;
using Telegram.Bot;

namespace BotCore.Domain.Implementations;

public class UnsubscribeCommandHandler(
    ISubscriptionRepository subscriptionsRepository,
    IUserSubscriptionRepository userSubscriptionsRepository) : ICommandHandler
{
    public string Command => CommandsConsts.Unsubscribe;

    public async Task HandleAsync(ITelegramBotClient bot, CommandContext context, CancellationToken ct)
    {
        var parts = context.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            await ShowUserSubscriptionsAsync(bot, context.ChatId, context.UserId, ct);
            return;
        }

        var subscriptionTypeId = parts[1];
        var subscriptionType = await subscriptionsRepository.FindByIdAsync(subscriptionTypeId, ct);

        if (subscriptionType is null)
        {
            await bot.SendMessage(
                context.ChatId,
                $"Подписки с id \"{subscriptionTypeId}\" не существует.\nПосмотреть доступные — просто /unsubscribe без аргумента.",
                replyMarkup: BotKeyboards.MainMenu,
                cancellationToken: ct);
            return;
        }

        await userSubscriptionsRepository.UnsubscribeAsync(context.UserId, subscriptionTypeId, ct);

        await bot.SendMessage(
            context.ChatId,
            $"Готово! Ты отписан от \"{subscriptionType.Name}\".",
            replyMarkup: BotKeyboards.MainMenu,
            cancellationToken: ct);
    }

    private async Task ShowUserSubscriptionsAsync(ITelegramBotClient bot, long chatId, long userId, CancellationToken ct)
    {
        var userSubscriptionIds = await userSubscriptionsRepository.GetUserSubscriptionTypeIdsAsync(userId, ct);

        if (userSubscriptionIds.Count == 0)
        {
            await bot.SendMessage(
                chatId,
                "У вас пока нет активных подписок.\nИспользуйте /subscribe, чтобы оформить подписку.",
                replyMarkup: BotKeyboards.MainMenu,
                cancellationToken: ct);
            return;
        }

        var allTypes = await subscriptionsRepository.GetAllAsync(ct);
        var userTypes = allTypes.Where(t => userSubscriptionIds.Contains(t.Id)).ToList();

        if (userTypes.Count == 0)
        {
            await bot.SendMessage(
                chatId,
                "У вас есть записи о подписках, но сами типы этих подписок были удалены администратором.",
                replyMarkup: BotKeyboards.MainMenu,
                cancellationToken: ct);
            return;
        }

        await bot.SendMessage(
            chatId,
            "Выберите подписку, от которой хотите отписаться:",
            replyMarkup: BotKeyboards.SubscriptionTypes(userTypes, CommandsConsts.Unsubscribe),
            cancellationToken: ct);
    }
}
