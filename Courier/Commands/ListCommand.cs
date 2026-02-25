using AutoCommand.Handler;
using Courier.Configuration;
using Courier.Extensions;
using Courier.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Serilog;

namespace Courier.Commands;

public class ListCommand(IOptionsMonitor<FeedOptions> optionsMonitor) : ICommandHandler
{
    public async Task HandleAsync(
        ILogger logger,
        SocketSlashCommand command,
        CancellationToken cancellationToken = new())
    {
        var feeds = optionsMonitor.CurrentValue.Feeds;
        var page = command.Data.Options.First(o => o.Name == "page")?.Value is long l
            ? l
            : 1;

        if (command.Data.Options.First(option => option.Name == "channel")?.Value is not IGuildChannel channel)
        {
            await command.RespondEphemeralAsync(
                Resources.ListCommandChannelOptionRequired,
                cancellationToken: cancellationToken);
            return;
        }

        var fields = feeds
            .Where(feed => feed.ChannelId == channel.Id)
            .SelectMany<Feed, EmbedFieldBuilder>(feed =>
            [
                new EmbedFieldBuilder()
                    .WithName("Name")
                    .WithValue(feed.Name.Truncate(EmbedFieldBuilder.MaxFieldValueLength))
                    .WithIsInline(false),
                new EmbedFieldBuilder()
                    .WithName("URI")
                    .WithValue(feed.Uri.Truncate(EmbedFieldBuilder.MaxFieldValueLength))
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName("Interval")
                    .WithValue(feed.Interval)
                    .WithIsInline(true)
            ])
            .Chunk(EmbedBuilder.MaxFieldCount)
            .Skip((int)(page - 1))
            .Take(1)
            .SelectMany(f => f);
        var embed = new EmbedBuilder()
            .WithTitle("Feeds")
            .WithColor(new Color(242, 125, 22))
            .WithFields(fields)
            .Build();

        await command.RespondAsync(
            embed: embed,
            ephemeral: true,
            options: new RequestOptions { CancelToken = cancellationToken });
    }

    public string CommandName => "list";
}