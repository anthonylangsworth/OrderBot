namespace EddnMessageProcessor
{
    /// <summary>
    /// Minor factions used to filter messages processed by <see cref="OrderBotMessageProcessor"/>.
    /// </summary>
    internal abstract class MinorFactionsSource
    {
        public abstract IReadOnlySet<string> Get();
    }
}
