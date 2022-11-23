using Discord;
using Discord.Interactions;
using OrderBot.Audit;

namespace OrderBot.Discord;

/// <summary>
/// An audit log that writes to a Discord channel.
/// </summary>
public class TextChannelAuditLogger : IAuditLogger
{
    private bool _disposedValue;
    private readonly TextChannelWriter _textChannelWriter;

    /// <summary>
    /// Create a new <see cref="TextChannelAuditLogger"/>.
    /// </summary>
    /// <param name="channelId">
    /// The channel ID to receive audit messages.
    /// </param>
    /// <param name="logger">
    /// Also log audit events.
    /// </param>
    /// <exception cref="ArgumentException">    
    /// Either <paramref name="context"/> is invalid or <paramref name="channelId"/> is not a valid channel.
    /// </exception>
    public TextChannelAuditLogger(SocketInteractionContext context, ulong channelId)
    {
        if (context.Guild.GetChannel(channelId) is not ITextChannel textChannel)
        {
            throw new ArgumentException(
                $"Either {nameof(context)} is invalid or {nameof(channelId)} {channelId} is not a valid channel");
        }
        _textChannelWriter = new TextChannelWriter(textChannel);
        UserName = context.Guild.GetUser(context.User.Id).DisplayName;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _textChannelWriter.Flush();
            _textChannelWriter.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// The user whose actions are being audited.
    /// </summary>
    public string UserName { get; }

    /// <inheritdoc/>
    public void Audit(string message)
    {
        _textChannelWriter.WriteLine($"{UserName}: {message}");
    }
}
