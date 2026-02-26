using AutoCommand.Handler;
using AutoCommand.Utils;
using Courier.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Courier.Services;

public class BotService(
    IOptions<BotOptions> options,
    ILogger<BotService> logger,
    DiscordSocketClient client,
    IEnumerable<ICommandHandler> commands)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var commandRouter = new DefaultCommandRouter(commands);

        client.Log += Log;
        client.Ready += async () =>
            await client.CreateSlashCommandsAsync(options.Value.FilePath, cancellationToken);
        client.SlashCommandExecuted += command => commandRouter.HandleAsync(command, cancellationToken);

        await client.LoginAsync(TokenType.Bot, options.Value.Token);
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite, cancellationToken);
        await client.StopAsync();
        await client.LogoutAsync();
    }

    private Task Log(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
                logger.LogCritical("{Message}", message.ToString());
                break;
            case LogSeverity.Error:
                logger.LogError("{Message}", message.ToString());
                break;
            case LogSeverity.Warning:
                logger.LogWarning("{Message}", message.ToString());
                break;
            case LogSeverity.Info:
                logger.LogInformation("{Message}", message.ToString());
                break;
            case LogSeverity.Verbose:
                logger.LogTrace("{Message}", message.ToString());
                break;
            case LogSeverity.Debug:
                logger.LogDebug("{Message}", message.ToString());
                break;
            default:
                logger.LogInformation("{Message}", message.ToString());
                break;
        }

        return Task.CompletedTask;
    }
}