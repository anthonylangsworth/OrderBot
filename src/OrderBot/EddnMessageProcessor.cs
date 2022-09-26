namespace OrderBot
{
    /// <summary>
    /// Process a message received by <see cref="EddnMessageBackgroundService"/>.
    /// </summary>
    internal abstract class EddnMessageProcessor
    {
        /// <summary>
        /// Process the <see cref="message"/>.
        /// </summary>
        /// <param name="message">
        /// The message to process.
        /// </param>
        public abstract void Process(string message);
    }
}
