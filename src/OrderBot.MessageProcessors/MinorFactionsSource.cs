namespace OrderBot.MessageProcessors
{
    /// <summary>
    /// Minor factions used to filter messages processed by <see cref="SystemMinorFactionMessageProcessor"/>.
    /// </summary>
    internal abstract class MinorFactionsSource
    {
        public abstract IReadOnlySet<string> Get();
    }
}
