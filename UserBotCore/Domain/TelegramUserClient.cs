using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserBotCore.Configuration;
using WTelegram;

namespace UserBotCore.Domain;

public class TelegramUserClient : IAsyncDisposable
{
    public Client Client { get; }
 
    private readonly UserBotConfiguration config;
    private readonly ILogger<TelegramUserClient> logger;
 
    public TelegramUserClient(IOptions<UserBotConfiguration> options, ILogger<TelegramUserClient> logger)
    {
        config = options.Value;
        this.logger = logger;
        
        Client = new Client(ConfigCallback);
    }
 
    // WTelegramClient дергает эту функцию сам, когда ему нужны данные для логина
    private string? ConfigCallback(string what) => what switch
    {
        "api_id" => config.ApiId.ToString(),
        "api_hash" => config.ApiHash,
        "phone_number" => config.Phone,
        "session_pathname" => config.SessionPath,
        // при первом запуске Telegram пришлёт код в само приложение/SMS —
        // вводим его прямо в консоли процесса
        "verification_code" => AskConsole("Введи код подтверждения из Telegram: "),
        // если на аккаунте включена двухфакторная аутентификация
        "password" => AskConsole("Введи пароль двухфакторной аутентификации: "),
        _ => null
    };
 
    private static string AskConsole(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }
 
    public async Task LoginAsync()
    {
        var user = await Client.LoginUserIfNeeded();
        logger.LogInformation("Userbot вошёл как {FirstName} (id {Id})", user.first_name, user.id);
    }
 
    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        return ValueTask.CompletedTask;
    }
}