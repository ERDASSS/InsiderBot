using Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotCore.Domain.Implementations;

public class HelpCommandHandler : ICommandHandler
{
    public string Command => CommandsConsts.Help;
    
    public async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        var helpText = """
                       📖 <b>Доступные команды:</b>

                       /start — Начать работу с ботом
                       /help — Показать это сообщение
                       /subscribe — Подписаться на рассылку
                       /unsubscribe — Отписаться от рассылки
                       /show — Показать ваши активные подписки

                       💡 <b>Примеры использования:</b>
                       • /subscribe 1 — подписаться на подписку с ID "1"
                       • /unsubscribe 1 — отписаться от подписки с ID "1"

                       Если у вас возникли вопросы, обратитесь к администратору.
                       """;

        await bot.SendMessage(
            chatId: message.Chat.Id,
            text: helpText,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            cancellationToken: ct);
    }
}