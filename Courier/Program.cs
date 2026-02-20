using AutoCommand.Handler;
using Courier.Commands;
using Courier.Configuration;
using Courier.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Courier;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.AddConsole();
        builder.Configuration.AddEnvironmentVariables($"{ConfKeys.Prefix}__");

        if (string.IsNullOrWhiteSpace(builder.Configuration[ConfKeys.Feed.FilePath]) ||
            !File.Exists(Path.GetFullPath(builder.Configuration[ConfKeys.Feed.FilePath]!)))
        {
            await Console.Error.WriteLineAsync(Resources.FeedsRequired);
            return;
        }

        builder.Configuration.AddJsonFile(builder.Configuration[ConfKeys.Feed.FilePath]!, false, true);

        builder.Services
            .AddOptionsWithValidateOnStart<BotOptions>()
            .Configure(options => { options.Token = builder.Configuration[ConfKeys.Bot.Token] ?? string.Empty; })
            .Validate(options => !string.IsNullOrWhiteSpace(options.Token), Resources.TokenRequired);

        builder.Services
            .AddOptionsWithValidateOnStart<FeedOptions>()
            .Bind(builder.Configuration)
            .Configure(options => options.FilePath = builder.Configuration[ConfKeys.Feed.FilePath] ?? string.Empty)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.FilePath) &&
                           File.Exists(Path.GetFullPath(options.FilePath)),
                Resources.FeedsRequired)
            .PostConfigure(options => { options.FilePath = Path.GetFullPath(options.FilePath); });

        builder.Services.AddTransient<ICommandHandler, AddFeedCommand>();
        builder.Services.AddTransient<ICommandHandler, RemoveCommand>();
        builder.Services.AddTransient<ICommandHandler, ListCommand>();
        builder.Services
            .AddSingleton(new DiscordSocketConfig { GatewayIntents = GatewayIntents.None })
            .AddSingleton<DiscordSocketClient>();
        builder.Services.AddHostedService<BotService>();
        builder.Services.AddHostedService<FeedService>();

        await builder.Build().RunAsync();
    }
}