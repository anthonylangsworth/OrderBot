using Discord;

namespace OrderBot.Discord;

/// <summary>
/// Buffer and split potentially large messages written to Discord text channels. Nothing is 
/// written until either <see cref="Flush"/> is called or written messages exceed
/// <see cref="DiscordConfig.MaxMessageSize"/> characters.
/// </summary>
public class TextChannelWriter : IDisposable
{
    private bool _disposedValue;
    private readonly TextChannelStream _discordChannelStream;
    private readonly BufferedStream _bufferedStream;
    private readonly StreamWriter _streamWriter;

    /// <summary>
    /// Create a <see cref="TextChannelWriter"/>.
    /// </summary>
    /// <param name="textChannel"></param>
    public TextChannelWriter(ITextChannel textChannel)
    {
        _discordChannelStream = new TextChannelStream(textChannel);
        _bufferedStream = new BufferedStream(_discordChannelStream, DiscordConfig.MaxMessageSize);
        _streamWriter = new StreamWriter(_bufferedStream);
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _streamWriter.Flush();
            _streamWriter.Dispose();
            _bufferedStream.Dispose();
            _discordChannelStream.Dispose();
            _disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Write any unwritten
    /// </summary>
    public void Flush()
    {
        _streamWriter.Flush();
    }

    /// <summary>
    /// Write a message to the text channel.
    /// </summary>
    /// <param name="message">
    /// The message to write.
    /// </param>
    public void WriteLine(string message)
    {
        _streamWriter.Write(message);
    }

    // Add other members, similar to StreamWriter, as needed
}
