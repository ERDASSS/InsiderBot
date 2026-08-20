using Contracts;
using Core.Domain;
using Core.Domain.BackgroundServices;
using Core.Domain.Implementations;
using Core.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<BotConfiguration>(options =>
{
    options.Token = builder.Configuration["BOT_TOKEN"]
                    ?? throw new InvalidOperationException("Variable BOT_TOKEN doesn't set");
});

builder.Services
    .AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var config = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;
        return new TelegramBotClient(config.Token, httpClient);
    });

builder.Services.AddScoped<ICommandHandler, StartCommandHandler>();


builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddHostedService<PollingService>();


var app = builder.Build();
await app.RunAsync();