using BotCore.Repository;
using Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Telegram.Bot;

namespace BotCore.Domain.BackgroundServices;

public class PostDispatcherService(
    IMongoDatabase database,
    ISubscriptionRepository subscriptionRepository,
    IUserSubscriptionRepository userSubscriptionRepository,
    ITelegramBotClient botClient,
    ILogger<PostDispatcherService> logger)
    : BackgroundService
{
    private readonly IMongoCollection<IncomingPost> incomingPosts = database.GetCollection<IncomingPost>("incoming_posts");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var unprocessedPosts = await incomingPosts
                    .Find(x => x.Processed == false)
                    .SortBy(x => x.ReceivedAt)
                    .ToListAsync(stoppingToken);

                foreach (var post in unprocessedPosts)
                {
                    var subscriptionTypes = await subscriptionRepository.GetByChannelIdAsync(post.ChannelId, stoppingToken);

                    if (subscriptionTypes.Count == 0)
                    {
                        await MarkAsProcessedAsync(post.Id, stoppingToken);
                        continue;
                    }

                    var notifiedUserIds = new HashSet<long>();

                    foreach (var subType in subscriptionTypes)
                    {
                        var userIds = await userSubscriptionRepository.GetSubscribedUserIdsAsync(subType.Id, stoppingToken);

                        foreach (var userId in userIds)
                        {
                            if (notifiedUserIds.Add(userId))
                            {
                                try
                                {
                                    await botClient.SendMessage(
                                        chatId: userId,
                                        text: post.Text,
                                        cancellationToken: stoppingToken);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogWarning(ex, "Не удалось отправить сообщение пользователю {UserId}", userId);
                                }
                            }
                        }
                    }

                    await MarkAsProcessedAsync(post.Id, stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Критическая ошибка в цикле PostDispatcherService");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task MarkAsProcessedAsync(string postId, CancellationToken ct)
    {
        await incomingPosts.UpdateOneAsync(
            x => x.Id == postId,
            Builders<IncomingPost>.Update.Set(x => x.Processed, true),
            cancellationToken: ct);
    }
}