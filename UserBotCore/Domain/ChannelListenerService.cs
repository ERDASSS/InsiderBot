using Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TL;

namespace UserBotCore.Domain;

public class ChannelListenerService : BackgroundService
{
    private readonly TelegramUserClient userClient;
    private readonly IMongoCollection<IncomingPost> incomingPosts;
    private readonly ILogger<ChannelListenerService> logger;
 
    public ChannelListenerService(
        TelegramUserClient userClient,
        IMongoDatabase database,
        ILogger<ChannelListenerService> logger)
    {
        this.userClient = userClient;
        incomingPosts = database.GetCollection<IncomingPost>("incoming_posts");
        this.logger = logger;
    }
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await userClient.LoginAsync();
 
        userClient.Client.OnUpdates += OnUpdatesAsync;
 
        logger.LogInformation("Userbot started collecting channels messages");
 
        // сам сервис ничего не делает в цикле — WTelegramClient работает
        // на своём фоновом соединении и просто дёргает OnUpdates по событию
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
 
    // WTelegramClient присылает пачку апдейтов за раз (UpdatesBase),
    // а не один Update — нужно развернуть их через UpdateList и отфильтровать
    private async Task OnUpdatesAsync(UpdatesBase updates)
    {
        foreach (var update in updates.UpdateList)
        {
            if (update is not UpdateNewChannelMessage { message: Message msg })
                continue;

            var channelTitle = updates.UserOrChat(msg.peer_id) is ChatBase chat
                ? chat.Title
                : null;
 
            var post = new IncomingPost
            {
                ChannelId = msg.peer_id.ID,
                ChannelTitle = channelTitle,
                Text = msg.message,
                ReceivedAt = DateTime.UtcNow,
                Processed = false
            };
 
            try
            {
                await incomingPosts.InsertOneAsync(post);
                logger.LogInformation(
                    "Сохранён новый пост из канала {ChannelTitle} ({ChannelId})",
                    post.ChannelTitle ?? "без названия",
                    post.ChannelId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Не удалось сохранить входящий пост в Mongo");
            }
        }
    }
}
