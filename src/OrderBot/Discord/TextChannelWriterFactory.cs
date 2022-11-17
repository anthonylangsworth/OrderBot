using Discord;

namespace OrderBot.Discord;

public class TextChannelWriterFactory : ITextChannelWriterFactory
{
    public TextChannelWriterFactory(IDiscordClient discordClient)
    {
        DiscordClient = discordClient;
    }

    public IDiscordClient DiscordClient { get; }

    /// <inheritdoc/>
    public async Task<TextChannelWriter> GetWriterAsync(ulong? channelId)
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
