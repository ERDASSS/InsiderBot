using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotCore.Domain.BackgroundServices;

public class PollingService(
    IServiceProvider serviceProvider,
    ITelegramBotClient botClient,
    ILogger<PollingService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await botClient.GetMe(stoppingToken);
        logger.LogInformation("Бот @{Username} запущен", me.Username);
 
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
 
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await botClient.ReceiveAsync(
                    updateHandler: HandleUpdateAsync,
                    errorHandler: HandleErrorAsync,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // нормальное завершение при остановке приложения
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка в цикле polling, перезапуск через 5 сек");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
 
    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var updateHandler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();
        await updateHandler.HandleAsync(bot, update, ct);
    }
 
    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Ошибка Telegram Bot API");
        return Task.CompletedTask;
    }
}