using Html2Markdown;

namespace Courier.Extensions;

public static class StringExtensions
{
    private static readonly Converter Converter = new();

    public static string ToMarkdown(this string s)
    {
        return Converter.Convert(s);
    }

    public static string Truncate(this string s, int maxLength)
    {
        return (s.Length > maxLength
            ? s[..(maxLength - 2)] + ".."
            : s).Trim();
    }
}