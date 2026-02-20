namespace Courier.Models;

public class Feed
{
    public required string Name { get; set; }
    public required string Uri { get; set; }
    public required ulong ChannelId { get; set; }
    public int Interval { get; set; } = 1;
    public DateTimeOffset LastUpdate { get; set; } = DateTimeOffset.UnixEpoch;
}