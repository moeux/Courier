using Courier.Models;
using Newtonsoft.Json;

namespace Courier.Utilities;

public static class JsonWriter
{
    public static async Task UpdateFeedsAsync(
        IEnumerable<Feed> feeds,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        await using var streamWriter = new StreamWriter(filePath);
        await using var jsonTextWriter = new JsonTextWriter(streamWriter);

        await jsonTextWriter.WriteStartObjectAsync(cancellationToken);
        await jsonTextWriter.WritePropertyNameAsync("Feeds", cancellationToken);
        await jsonTextWriter.WriteStartArrayAsync(cancellationToken);

        foreach (var feed in feeds)
        {
            await jsonTextWriter.WriteStartObjectAsync(cancellationToken);
            await jsonTextWriter.WritePropertyNameAsync("Name", cancellationToken);
            await jsonTextWriter.WriteValueAsync(feed.Name, cancellationToken);
            await jsonTextWriter.WritePropertyNameAsync("Uri", cancellationToken);
            await jsonTextWriter.WriteValueAsync(feed.Uri, cancellationToken);
            await jsonTextWriter.WritePropertyNameAsync("Channel", cancellationToken);
            await jsonTextWriter.WriteValueAsync(feed.ChannelId, cancellationToken);
            await jsonTextWriter.WritePropertyNameAsync("Interval", cancellationToken);
            await jsonTextWriter.WriteValueAsync(feed.Interval, cancellationToken);
            await jsonTextWriter.WriteEndObjectAsync(cancellationToken);
        }

        await jsonTextWriter.WriteEndArrayAsync(cancellationToken);
        await jsonTextWriter.WriteEndObjectAsync(cancellationToken);
    }
}