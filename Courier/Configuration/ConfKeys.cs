namespace Courier.Configuration;

public static class ConfKeys
{
    public const string Prefix = "Courier";

    public static class Bot
    {
        public const string Token = $"{Prefix}:{nameof(Bot)}:{nameof(Token)}";
    }

    public static class Feed
    {
        public const string FilePath = $"{Prefix}:{nameof(Feed)}:{nameof(FilePath)}";
    }
}