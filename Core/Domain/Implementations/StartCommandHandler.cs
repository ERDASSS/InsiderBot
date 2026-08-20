using Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Domain.Implementations;

public class StartCommandHandler : ICommandHandler
{
    public string Command => CommandsConsts.Start;
    
    public Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        return bot.SendMessage(
            chatId: message.Chat.Id,
            text: "Тестовый запуск",
            cancellationToken: ct);
    }
}