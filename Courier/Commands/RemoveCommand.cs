using AutoCommand.Handler;
using Courier.Configuration;
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
            await command.RespondAsync(
                Resources.RemoveCommandChannelOptionRequired,
                ephemeral: true,
                options: new RequestOptions
                {
                    CancelToken = cancellationToken
                });
            return;
        }

        if (command.Data.Options.First(option => option.Name == "name")?.Value is not string name ||
            string.IsNullOrWhiteSpace(name))
        {
            await command.RespondAsync(
                Resources.RemoveCommandNameOptionRequired,
                ephemeral: true,
                options: new RequestOptions
                {
                    CancelToken = cancellationToken
                });
            return;
        }

        var feeds = new List<Feed>(optionsMonitor.CurrentValue.Feeds);
        var removedFeeds = feeds.RemoveAll(feed => feed.Name == name && feed.ChannelId == channel.Id);

        if (removedFeeds == 0)
        {
            await command.RespondAsync(
                Resources.RemoveCommandNoFeedsFound,
                ephemeral: true,
                options: new RequestOptions
                {
                    CancelToken = cancellationToken
                });
            return;
        }

        await JsonWriter.UpdateFeedsAsync(feeds, optionsMonitor.CurrentValue.FilePath, cancellationToken);
        await command.RespondAsync(
            Resources.RemoveCommandFeedRemoved,
            ephemeral: true,
            options: new RequestOptions
            {
                CancelToken = cancellationToken
            });
    }

    public string CommandName => "remove";
}