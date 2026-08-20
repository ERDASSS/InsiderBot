using BotCore.Repository;
using Contracts;
using Telegram.Bot;

namespace BotCore.Domain.Implementations;

public class SubscribeCommandHandler(
    ISubscriptionRepository subscriptionsRepository,
    IUserSubscriptionRepository userSubscriptionsRepository)
    : ICommandHandler
{
    public string Command => CommandsConsts.Subscribe;

    public async Task HandleAsync(ITelegramBotClient bot, CommandContext context, CancellationToken ct)
    {
        var parts = context.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            await ShowAvailableSubscriptionsAsync(bot, context.ChatId, ct);
            return;
        }

        var subscriptionTypeId = parts[1];
        var subscriptionType = await subscriptionsRepository.FindByIdAsync(subscriptionTypeId, ct);

        if (subscriptionType is null)
        {
            await bot.SendMessage(
                context.ChatId,
                $"Подписки с id \"{subscriptionTypeId}\" не существует.\nПосмотреть доступные — просто /subscribe без аргумента.",
                replyMarkup: BotKeyboards.MainMenu,
                cancellationToken: ct);
            return;
        }

        await userSubscriptionsRepository.SubscribeAsync(context.UserId, subscriptionTypeId, ct);

        await bot.SendMessage(
            context.ChatId,
            $"Готово! Ты подписан на \"{subscriptionType.Name}\".",
            replyMarkup: BotKeyboards.MainMenu,
            cancellationToken: ct);
    }

    private async Task ShowAvailableSubscriptionsAsync(ITelegramBotClient bot, long chatId, CancellationToken ct)
    {
        var types = await subscriptionsRepository.GetAllAsync(ct);

        if (types.Count == 0)
        {
            await bot.SendMessage(
                chatId,
                "Пока нет доступных подписок.",
                replyMarkup: BotKeyboards.MainMenu,
                cancellationToken: ct);
            return;
        }

        await bot.SendMessage(
            chatId,
            "Выберите подписку:",
            replyMarkup: BotKeyboards.SubscriptionTypes(types, CommandsConsts.Subscribe),
            cancellationToken: ct);
    }
}
