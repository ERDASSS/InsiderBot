using Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotCore.Domain;

public class UpdateHandler(IEnumerable<ICommandHandler> commandHandlers)
{
    public async Task HandleAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is { } callbackQuery)
        {
            await HandleCallbackQueryAsync(bot, callbackQuery, ct);
            return;
        }

        if (update.Type != UpdateType.Message || update.Message?.Text is not { } text)
            return;

        var message = update.Message;
        var commandText = BotKeyboards.TryResolveMenuCommand(text) ?? text;
        var context = new CommandContext(
            message.Chat.Id,
            message.From?.Id ?? message.Chat.Id,
            commandText);

        if (await TryHandleCommandAsync(bot, context, ct))
            return;

        await bot.SendMessage(
            message.Chat.Id,
            "Команда не найдена. Выберите действие кнопками снизу.",
            replyMarkup: BotKeyboards.MainMenu,
            cancellationToken: ct);
    }

    private async Task HandleCallbackQueryAsync(
        ITelegramBotClient bot,
        CallbackQuery callbackQuery,
        CancellationToken ct)
    {
        await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

        if (callbackQuery.Data is not { } data || string.IsNullOrWhiteSpace(data))
            return;

        var chatId = callbackQuery.Message?.Chat.Id ?? callbackQuery.From.Id;
        var context = new CommandContext(chatId, callbackQuery.From.Id, data);

        if (await TryHandleCommandAsync(bot, context, ct))
            return;

        await bot.SendMessage(
            chatId,
            "Действие не найдено. Попробуйте выбрать команду в меню.",
            replyMarkup: BotKeyboards.MainMenu,
            cancellationToken: ct);
    }

    private async Task<bool> TryHandleCommandAsync(
        ITelegramBotClient bot,
        CommandContext context,
        CancellationToken ct)
    {
        var text = context.Text.Trim();

        if (!text.StartsWith('/'))
            return false;

        var command = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].ToLowerInvariant();
        var handler = commandHandlers.FirstOrDefault(h => h.Command == command);

        if (handler is null)
            return false;

        await handler.HandleAsync(bot, context, ct);
        return true;
    }
}
