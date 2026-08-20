using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core.Domain.BackgroundServices;

public class UpdateHandler
{
    private readonly IEnumerable<ICommandHandler> commandHandlers;
 
    public UpdateHandler(IEnumerable<ICommandHandler> commandHandlers)
    {
        this.commandHandlers = commandHandlers;
    }
 
    public async Task HandleAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Type != UpdateType.Message || update.Message?.Text is not { } text)
            return; 
 
        var message = update.Message;
 
        if (text.StartsWith('/'))
        {
            var command = text.Split(' ')[0].ToLowerInvariant();
            var handler = commandHandlers.FirstOrDefault(h => h.Command == command);
 
            if (handler is not null)
            {
                await handler.HandleAsync(bot, message, ct);
            }
        }
        // // Если команда не найдена или это обычный текст — ответ по умолчанию
        // await bot.SendMessage(message.Chat.Id, $"Ты написал: {text}", cancellationToken: ct);
    }
}