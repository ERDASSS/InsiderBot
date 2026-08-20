using Contracts;
using Telegram.Bot;

namespace BotCore.Domain.Implementations;

public class StartCommandHandler : ICommandHandler
{
    public string Command => CommandsConsts.Start;
    
    public Task HandleAsync(ITelegramBotClient bot, CommandContext context, CancellationToken ct)
    {
        return bot.SendMessage(
            chatId: context.ChatId,
            text: "Бот запущен. Выберите действие кнопками снизу.",
            replyMarkup: BotKeyboards.MainMenu,
            cancellationToken: ct);
    }
}
