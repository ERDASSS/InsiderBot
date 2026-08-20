using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserBotCore.Configuration;
using UserBotCore.Domain;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<UserBotConfiguration>(options =>
{
    options.ApiId = int.Parse(builder.Configuration["TG_API_ID"]
                              ?? throw new InvalidOperationException("Variable TG_API_ID doesn't set"));
    options.ApiHash = builder.Configuration["TG_API_HASH"]
                      ?? throw new InvalidOperationException("Variable TG_API_HASH doesn't set");
    options.Phone = builder.Configuration["TG_PHONE"]
                    ?? throw new InvalidOperationException("Variable TG_PHONE doesn't set");
    options.SessionPath = builder.Configuration["TG_SESSION_PATH"] ?? "userbot.session";
});

builder.Services.Configure<MongoSettings>(options =>
{
    options.ConnectionString = builder.Configuration["MONGO_CONNECTION_STRING"]
                               ?? throw new InvalidOperationException("Variable MONGO_CONNECTION_STRING doesn't set");
    options.DatabaseName = builder.Configuration["MONGO_DATABASE_NAME"]
                           ?? throw new InvalidOperationException("Variable MONGO_DATABASE_NAME doesn't set");
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});

// --- Telegram userbot ---
builder.Services.AddSingleton<TelegramUserClient>();
builder.Services.AddHostedService<ChannelListenerService>();

var app = builder.Build();
await app.RunAsync();