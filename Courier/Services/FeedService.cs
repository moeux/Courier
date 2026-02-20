using System.ServiceModel.Syndication;
using System.Xml;
using Courier.Configuration;
using Courier.Extensions;
using Courier.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace Courier.Services;

public class FeedService(IOptions<FeedOptions> options, ILogger<FeedService> logger, DiscordSocketClient client)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var feedTimerMap = options.Value.Feeds.ToDictionary(
            feed => feed,
            feed =>
            {
                var timer = new Timer(TimeSpan.FromMinutes(feed.Interval));
                timer.Elapsed += async (sender, _) => await ProcessFeed(sender as Timer, feed, cancellationToken);
                timer.Enabled = true;

                logger.LogInformation(
                    "Setup feed '{Name}' ({Uri}) with interval {Interval} minutes and channel #{ChannelId}.",
                    feed.Name, feed.Uri, feed.Interval, feed.ChannelId);

                return timer;
            });

        await Task.Delay(Timeout.Infinite, cancellationToken);

        foreach (var timer in feedTimerMap.Values) timer.Dispose();
    }

    private async Task ProcessFeed(Timer? timer, Feed feed, CancellationToken cancellationToken)
    {
        timer?.Stop();

        try
        {
            var response = await GetFeed(feed.Uri, cancellationToken);
            using var reader =
                XmlReader.Create(new StringReader(response), new XmlReaderSettings { CloseInput = true });
            var syndicationFeed = SyndicationFeed.Load(reader);

            syndicationFeed.Items = syndicationFeed.Items
                .Where(item => item.PublishDate <= DateTimeOffset.UtcNow)
                .Where(item => item.PublishDate >= feed.LastUpdate).ToList();

            if (!syndicationFeed.Items.Any()) return;

            logger.LogInformation("Sending {Count} feed items to channel #{ChannelId}.",
                syndicationFeed.Items.Count(), feed.ChannelId);

            await SendMessage(feed.ChannelId, syndicationFeed, cancellationToken);
            feed.LastUpdate = DateTimeOffset.UtcNow;
        }
        finally
        {
            timer?.Start();
        }
    }

    private static async Task<string> GetFeed(string uri, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync(uri, cancellationToken);
        return response.Replace("&shy;", "");
    }

    private async Task SendMessage(ulong channelId, SyndicationFeed feed, CancellationToken cancellationToken = default)
    {
        var channel = await client.GetChannelAsync(channelId, new RequestOptions
        {
            Timeout = DiscordConfig.DefaultRequestTimeout,
            CancelToken = cancellationToken,
            RetryMode = RetryMode.AlwaysRetry
        });
        var embedBuilder = new EmbedBuilder();
        var embeds = new List<Embed>();

        if (channel is not ITextChannel textChannel) return;

        foreach (var item in feed.Items)
        {
            if (cancellationToken.IsCancellationRequested) break;

            embedBuilder
                .WithAuthor(authorBuilder => authorBuilder
                    .WithName(feed.Title.Text.ToMarkdown().Truncate(EmbedAuthorBuilder.MaxAuthorNameLength))
                    .WithIconUrl(feed.ImageUrl.ToString()))
                .WithFooter(feed.Description.Text.ToMarkdown().Truncate(EmbedFooterBuilder.MaxFooterTextLength))
                .WithUrl(item.Links.Count != 0 ? item.Links.First().Uri.ToString() : item.Id)
                .WithTitle(item.Title.Text.ToMarkdown().Truncate(EmbedBuilder.MaxTitleLength))
                .WithDescription(item.Summary.Text.ToMarkdown().Truncate(EmbedBuilder.MaxDescriptionLength))
                .WithTimestamp(item.PublishDate)
                .WithColor(Color.Purple);

            embeds.Add(embedBuilder.Build());
        }

        await textChannel.SendMessageAsync(embeds: embeds.ToArray(), options: new RequestOptions
        {
            Timeout = DiscordConfig.DefaultRequestTimeout,
            CancelToken = cancellationToken,
            RetryMode = RetryMode.AlwaysRetry
        });
    }
}