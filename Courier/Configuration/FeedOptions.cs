using Courier.Models;

namespace Courier.Configuration;

public class FeedOptions
{
    public required string FilePath { get; set; }
    public required Feed[] Feeds { get; set; }
}