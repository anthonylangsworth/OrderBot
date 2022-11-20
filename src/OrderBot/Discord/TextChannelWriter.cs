using Discord;
using System.Text;

namespace OrderBot.Discord;

/// <summary>
/// Buffer and split potentially large messages written to Discord text channels. Nothing is 
/// written until either <see cref="Flush"/> is called or written messages exceed
/// <see cref="DiscordConfig.MaxMessageSize"/> characters.
/// </summary>
public class TextChannelWriter : TextWriter, IDisposable
{
    private bool _disposedValue;
    private readonly TextChannelStream _discordChannelStream;
    private readonly BufferedStream _bufferedStream;
    private readonly StreamWriter _streamWriter;
    public override Encoding Encoding => Encoding.UTF8;

    /// <summary>
    /// Create a <see cref="TextChannelWriter"/>.
    /// </summary>
    /// <param name="textChannel"></param>
    public TextChannelWriter(ITextChannel textChannel)
    {
        _discordChannelStream = new TextChannelStream(textChannel);
        _bufferedStream = new BufferedStream(_discordChannelStream, global::Discord.DiscordConfig.MaxMessageSize);
        _streamWriter = new StreamWriter(_bufferedStream);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!_disposedValue)
        {
            _streamWriter.Flush();
            _streamWriter.Dispose();
            _bufferedStream.Dispose();
            _discordChannelStream.Dispose();
            _disposedValue = true;
        }
    }

    /// <summary>
    /// Write any unwritten
    /// </summary>
    public override void Flush()
    {
        _streamWriter.Flush();
    }

    /// <summary>
    /// Write a character to the text channel..
    /// </summary>
    /// <param name="message">
    /// The message to write.
    /// </param>
    public override void Write(char character)
    {
        _streamWriter.Write(character);
    }

    // Add other members, similar to StreamWriter, as needed
}
