using System.Runtime.Serialization;

namespace OrderBot.Discord;

/// <summary>
/// Thrown by a Discord command to return an error message to the user.
/// </summary>
[Serializable]
internal class DiscordUserInteractionException : Exception
{
    /// <inheritdoc/>
    public DiscordUserInteractionException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc/>
    public DiscordUserInteractionException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc/>
    protected DiscordUserInteractionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        // Do nothing
    }
}
