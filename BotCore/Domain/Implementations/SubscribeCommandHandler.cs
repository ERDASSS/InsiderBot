using BotCore.Repository;
using Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotCore.Domain.Implementations;

public class SubscribeCommandHandler(
    ISubscriptionRepository subscriptionsRepository,
    IUserSubscriptionRepository userSubscriptionsRepository)
    : ICommandHandler
{
    public string Command => CommandsConsts.Subscribe;

    public async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        var parts = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 2)
        {
            await ShowAvailableSubscriptionsAsync(bot, message.Chat.Id, ct);
            return;
        }
        
        var subscriptionTypeId = parts[1];
        var subscriptionType = await subscriptionsRepository.FindByIdAsync(subscriptionTypeId, ct);
 
        if (subscriptionType is null)
        {
            await bot.SendMessage(
                message.Chat.Id,
                $"Подписки с id \"{subscriptionTypeId}\" не существует.\nПосмотреть доступные — просто /subscribe без аргумента.",
                cancellationToken: ct);
            return;
        }
 
        await userSubscriptionsRepository.SubscribeAsync(message.From!.Id, subscriptionTypeId, ct);
 
        await bot.SendMessage(
            message.Chat.Id,
            $"Готово! Ты подписан на \"{subscriptionType.Name}\".",
            cancellationToken: ct);
    }
 
    private async Task ShowAvailableSubscriptionsAsync(ITelegramBotClient bot, long chatId, CancellationToken ct)
    {
        var types = await subscriptionsRepository.GetAllAsync(ct);
 
        if (types.Count == 0)
        {
            await bot.SendMessage(chatId, "Пока нет доступных подписок.", cancellationToken: ct);
            return;
        }
 
        var list = string.Join('\n', types.Select(t => $"{t.Id} — {t.Name}"));
 
        await bot.SendMessage(
            chatId,
            $"Доступные подписки:\n{list}\n\nЧтобы подписаться, напиши команду:\n/subscribe <id>",
            cancellationToken: ct);
    }
}