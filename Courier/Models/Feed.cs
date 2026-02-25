namespace Courier.Models;

public class Feed
{
    public const long DefaultInterval = 60;

    public required string Name { get; init; }
    public required string Uri { get; init; }
    public required ulong ChannelId { get; init; }
    public long Interval { get; init; } = DefaultInterval;
    public DateTimeOffset LastUpdate { get; set; } = DateTimeOffset.UnixEpoch;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not Feed other) return false;
        return Name == other.Name && Uri == other.Uri && ChannelId == other.ChannelId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Uri, ChannelId);
    }
}