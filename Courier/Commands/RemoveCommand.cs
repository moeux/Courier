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

public class RemoveCommand(IOptionsMonitor<FeedOptions> optionsMonitor) : ICommandHandler
{
    public async Task HandleAsync(
        ILogger logger,
        SocketSlashCommand command,
        CancellationToken cancellationToken = new())
    {
        if (command.Data.Options.First(option => option.Name == "channel")?.Value is not IGuildChannel channel)
        {
            await command.RespondEphemeralAsync(
                Resources.RemoveCommandChannelOptionRequired,
                cancellationToken: cancellationToken);
            return;
        }

        if (command.Data.Options.First(option => option.Name == "name")?.Value is not string name ||
            string.IsNullOrWhiteSpace(name))
        {
            await command.RespondEphemeralAsync(
                Resources.RemoveCommandNameOptionRequired,
                cancellationToken: cancellationToken);
            return;
        }

        var feeds = new List<Feed>(optionsMonitor.CurrentValue.Feeds);
        var removedFeeds = feeds.RemoveAll(feed => feed.Name == name && feed.ChannelId == channel.Id);

        if (removedFeeds == 0)
        {
            await command.RespondEphemeralAsync(
                Resources.RemoveCommandNoFeedsFound,
                cancellationToken: cancellationToken);
            return;
        }

        await JsonWriter.UpdateFeedsAsync(feeds, optionsMonitor.CurrentValue.FilePath, cancellationToken);
        await command.RespondEphemeralAsync(
            Resources.RemoveCommandFeedRemoved,
            cancellationToken: cancellationToken);
    }

    public string CommandName => "remove";
}