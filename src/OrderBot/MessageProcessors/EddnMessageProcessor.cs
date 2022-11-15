using System.Text.Json;

namespace OrderBot.MessageProcessors;

/// <summary>
/// Process a message received by <see cref="EddnMessageHostedService"/>.
/// </summary>
public abstract class EddnMessageProcessor
{
    /// <summary>
    /// Process the <see cref="message"/>.
    /// </summary>
    /// <param name="message">
    /// The message to process.
    /// </param>
    public abstract Task ProcessAsync(JsonDocument message);
}
