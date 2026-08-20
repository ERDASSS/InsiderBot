using Contracts;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotCore.Domain;

public static class BotKeyboards
{
    public const string SubscribeButton = "Подписаться";
    public const string UnsubscribeButton = "Отписаться";
    public const string ShowButton = "Мои подписки";
    public const string HelpButton = "Помощь";

    public static ReplyKeyboardMarkup MainMenu { get; } = new(new[]
    {
        new[] { new KeyboardButton(SubscribeButton), new KeyboardButton(UnsubscribeButton) },
        new[] { new KeyboardButton(ShowButton), new KeyboardButton(HelpButton) }
    })
    {
        ResizeKeyboard = true,
        IsPersistent = true,
        InputFieldPlaceholder = "Выберите действие"
    };

    public static string? TryResolveMenuCommand(string text)
    {
        return text.Trim() switch
        {
            SubscribeButton => CommandsConsts.Subscribe,
            UnsubscribeButton => CommandsConsts.Unsubscribe,
            ShowButton => CommandsConsts.Show,
            HelpButton => CommandsConsts.Help,
            _ => null
        };
    }

    public static InlineKeyboardMarkup SubscriptionTypes(
        IEnumerable<SubscriptionType> subscriptionTypes,
        string command)
    {
        var rows = subscriptionTypes.Select(type => new[]
        {
            InlineKeyboardButton.WithCallbackData(
                $"{type.Name} ({type.Id})",
                $"{command} {type.Id}")
        });

        return new InlineKeyboardMarkup(rows);
    }
}
