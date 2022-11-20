using Discord;

namespace OrderBot.Discord;

public class TextChannelWriterFactory
{
    public TextChannelWriterFactory(IDiscordClient discordClient)
    {
        DiscordClient = discordClient;
    }

    public IDiscordClient DiscordClient { get; }

    /// <summary>
    /// Get a <see cref="TextChannelWriter"/> for the given channel.
    /// </summary>
    /// <param name="channelId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    /// Either <see cref="DiscordClient"/> is not connected.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="channelId"/> is not a valid Discord text channel.
    /// </exception>
    public virtual async Task<TextWriter> GetWriterAsync(ulong? channelId)
    {
        if (await DiscordClient.GetChannelAsync(channelId ?? 0) is ITextChannel textChannel)
        {
            return new TextChannelWriter(textChannel);
        }
        else
        {
            throw new ArgumentException($"{channelId} is not a Discord text channel");
        }
    }
}
