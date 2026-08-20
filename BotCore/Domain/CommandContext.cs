namespace BotCore.Domain;

public record CommandContext(long ChatId, long UserId, string Text);
