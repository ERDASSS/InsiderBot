using BotCore.Configuration;
using BotCore.Domain;
using BotCore.Domain.BackgroundServices;
using BotCore.Domain.Implementations;
using BotCore.Repository;
using BotCore.Repository.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<BotConfiguration>(options =>
{
    options.Token = builder.Configuration["BOT_TOKEN"]
                    ?? throw new InvalidOperationException("Variable BOT_TOKEN doesn't set");
});

builder.Services.Configure<MongoSettings>(options =>
{
    options.ConnectionString = builder.Configuration["MONGO_CONNECTION_STRING"]
                               ?? throw new InvalidOperationException("Variable MONGO_CONNECTION_STRING doesn't set");
    options.DatabaseName = builder.Configuration["MONGO_DATABASE_NAME"]
                           ?? throw new InvalidOperationException("Variable MONGO_DATABASE_NAME doesn't set");
});

builder.Services
    .AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var config = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;
        return new TelegramBotClient(config.Token, httpClient);
    });

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<ISubscriptionRepository, MongoSubscriptionRepository>();
builder.Services.AddSingleton<IUserSubscriptionRepository, MongoUserSubscriptionRepository>();

builder.Services.AddScoped<ICommandHandler, StartCommandHandler>();
builder.Services.AddScoped<ICommandHandler, SubscribeCommandHandler>();
builder.Services.AddScoped<ICommandHandler, UnsubscribeCommandHandler>();
builder.Services.AddScoped<ICommandHandler, HelpCommandHandler>();
builder.Services.AddScoped<ICommandHandler, ShowCommandHandler>();

builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddHostedService<PollingService>();
builder.Services.AddHostedService<PostDispatcherService>();

var app = builder.Build();
await app.RunAsync();