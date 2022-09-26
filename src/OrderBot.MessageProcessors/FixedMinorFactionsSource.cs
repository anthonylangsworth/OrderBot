namespace OrderBot.MessageProcessors
{
    /// <summary>
    /// Use a fixed set of minor factions.
    /// </summary>
    internal class FixedMinorFactionsSource : MinorFactionsSource
    {
        public FixedMinorFactionsSource(IReadOnlySet<string> minorFactions)
        {
            MinorFactions = minorFactions;
        }

        public IReadOnlySet<string> MinorFactions { get; }

        public override IReadOnlySet<string> Get()
        {
            return MinorFactions;
        }
    }
}
