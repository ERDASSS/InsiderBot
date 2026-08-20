using Telegram.Bot;

namespace BotCore.Domain;

public interface ICommandHandler
{
    string Command { get; }
 
    Task HandleAsync(ITelegramBotClient bot, CommandContext context, CancellationToken ct);
}
