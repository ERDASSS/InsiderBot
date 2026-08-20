using Contracts;
using Telegram.Bot;

namespace BotCore.Domain.Implementations;

public class HelpCommandHandler : ICommandHandler
{
    public string Command => CommandsConsts.Help;
    
    public async Task HandleAsync(ITelegramBotClient bot, CommandContext context, CancellationToken ct)
    {
        var helpText = """
                       📖 <b>Доступные действия:</b>

                       Подписаться — выбрать подписку на рассылку
                       Отписаться — выбрать подписку для отключения
                       Мои подписки — показать активные подписки
                       Помощь — показать это сообщение

                       Текстовые команды тоже поддерживаются: /subscribe, /unsubscribe, /show.
                       """;

        await bot.SendMessage(
            chatId: context.ChatId,
            text: helpText,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: BotKeyboards.MainMenu,
            cancellationToken: ct);
    }
}
