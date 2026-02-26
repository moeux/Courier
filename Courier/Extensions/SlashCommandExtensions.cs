using Discord;
using Discord.WebSocket;

namespace Courier.Extensions;

public static class SlashCommandExtensions
{
    public static async Task RespondEphemeralAsync(
        this SocketSlashCommand command,
        string? text = null,
        Embed? embed = null,
        CancellationToken cancellationToken = default)
    {
        await command.RespondAsync(
            text,
            embed: embed,
            ephemeral: true,
            options: new RequestOptions { CancelToken = cancellationToken });
    }
}