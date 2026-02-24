namespace Courier.Models;

public class Feed
{
    public const long DefaultInterval = 60;

    public required string Name { get; set; }
    public required string Uri { get; set; }
    public required ulong ChannelId { get; set; }
    public long Interval { get; set; } = DefaultInterval;
    public DateTimeOffset LastUpdate { get; set; } = DateTimeOffset.UnixEpoch;
}