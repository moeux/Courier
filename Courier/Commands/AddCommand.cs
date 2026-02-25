using AutoCommand.Handler;
using Courier.Configuration;
using Courier.Extensions;
using Courier.Models;
using Courier.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Serilog;

namespace Courier.Commands;

public class AddCommand(IOptionsMonitor<FeedOptions> optionsMonitor, DiscordSocketClient client) : ICommandHandler
{
    public async Task HandleAsync(
        ILogger logger,
        SocketSlashCommand command,
        CancellationToken cancellationToken = new())
    {
        var commandOptions = command.Data.Options;
        var interval = command.Data.Options
            .Where(option => option.Name == "interval")
            .Select(option => option.Value)
            .Cast<long>()
            .FirstOrDefault(Feed.DefaultInterval);

        if (!Validate<string>(commandOptions.First(option => option.Name == "name"), out var name) ||
            name is null)
        {
            await command.RespondEphemeralAsync(
                Resources.AddCommandNameOptionRequired,
                cancellationToken: cancellationToken);
            return;
        }

        if (!Validate<Uri>(commandOptions.First(option => option.Name == "uri"), out var uri) ||
            uri is null)
        {
            await command.RespondEphemeralAsync(
                Resources.AddCommandUriOptionRequired,
                cancellationToken: cancellationToken);
            return;
        }

        if (!Validate<IGuildChannel>(commandOptions.First(option => option.Name == "channel"), out var channel) ||
            channel is null)
        {
            await command.RespondEphemeralAsync(
                Resources.AddCommandChannelOptionRequired,
                cancellationToken: cancellationToken);
            return;
        }

        var feeds = new List<Feed>(optionsMonitor.CurrentValue.Feeds)
        {
            new()
            {
                Name = name,
                Uri = uri.AbsoluteUri,
                ChannelId = channel.Id,
                Interval = interval
            }
        };

        await JsonWriter.UpdateFeedsAsync(feeds, optionsMonitor.CurrentValue.FilePath, cancellationToken);
        await command.RespondEphemeralAsync(
            Resources.AddCommandFeedAdded,
            cancellationToken: cancellationToken);
    }

    public string CommandName => "add";

    private bool Validate<T>(SocketSlashCommandDataOption option, out T? value) where T : class
    {
        switch (option.Value)
        {
            case string s when typeof(T) == typeof(string):
                value = Convert.ChangeType(s, typeof(T)) as T;
                return !string.IsNullOrWhiteSpace(s);
            case string u when typeof(T) == typeof(Uri):
                var isValid = Uri.TryCreate(u.Trim(), UriKind.Absolute, out var uri);
                value = Convert.ChangeType(uri?.AbsolutePath, typeof(T)) as T;
                return isValid && (uri?.Scheme == Uri.UriSchemeHttp || uri?.Scheme == Uri.UriSchemeHttps);
            case long l when typeof(T) == typeof(long):
                value = Convert.ChangeType(l, typeof(T)) as T;
                return l > 0;
            case ITextChannel c when typeof(T) == typeof(IGuildChannel):
                value = Convert.ChangeType(c, typeof(T)) as T;
                return c.GetPermissionOverwrite(client.CurrentUser) is
                {
                    SendMessages: PermValue.Allow,
                    EmbedLinks: PermValue.Allow
                };
        }

        value = null;
        return false;
    }
}