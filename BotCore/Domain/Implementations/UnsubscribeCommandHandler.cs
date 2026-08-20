using BotCore.Repository;
using Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotCore.Domain.Implementations;

public class UnsubscribeCommandHandler(
    ISubscriptionRepository subscriptionsRepository,
    IUserSubscriptionRepository userSubscriptionsRepository) : ICommandHandler
{
    public string Command => CommandsConsts.Unsubscribe;
    
    public async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        var parts = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 2)
        {
            await ShowUserSubscriptionsAsync(bot, message.Chat.Id, ct);
            return;
        }
        
        var subscriptionTypeId = parts[1];
        var subscriptionType = await subscriptionsRepository.FindByIdAsync(subscriptionTypeId, ct);
 
        if (subscriptionType is null)
        {
            await bot.SendMessage(
                message.Chat.Id,
                $"Подписки с id \"{subscriptionTypeId}\" не существует.\nПосмотреть доступные — просто /unsubscribe без аргумента.",
                cancellationToken: ct);
            return;
        }
 
        await userSubscriptionsRepository.UnsubscribeAsync(message.From!.Id, subscriptionTypeId, ct);
 
        await bot.SendMessage(
            message.Chat.Id,
            $"Готово! Ты отписан от \"{subscriptionType.Name}\".",
            cancellationToken: ct);
    }
    
    private async Task ShowUserSubscriptionsAsync(ITelegramBotClient bot, long userId, CancellationToken ct)
    {
        var userSubscriptionIds = await userSubscriptionsRepository.GetUserSubscriptionTypeIdsAsync(userId, ct);
 
        if (userSubscriptionIds.Count == 0)
        {
            await bot.SendMessage(
                userId, 
                "У вас пока нет активных подписок.\nИспользуйте /subscribe, чтобы оформить подписку.", 
                cancellationToken: ct);
            return;
        }
        
        var allTypes = await subscriptionsRepository.GetAllAsync(ct);
        
        var userTypes = allTypes.Where(t => userSubscriptionIds.Contains(t.Id)).ToList();

        if (userTypes.Count == 0)
        {
            await bot.SendMessage(
                userId,
                "У вас есть записи о подписках, но сами типы этих подписок были удалены администратором.",
                cancellationToken: ct);
            return;
        }
        
        var list = string.Join('\n', userTypes.Select(t => $"🔹 {t.Id} — {t.Name}"));
 
        await bot.SendMessage(
            userId,
            $"Ваши активные подписки:\n\n{list}\n\nЧтобы отписаться, напиши команду:\n/unsubscribe <id>",
            cancellationToken: ct);
    }
}