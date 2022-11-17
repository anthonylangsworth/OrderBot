namespace OrderBot.Discord;

public interface ITextChannelWriterFactory
{
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
    Task<TextChannelWriter> GetWriterAsync(ulong? channelId);
}
