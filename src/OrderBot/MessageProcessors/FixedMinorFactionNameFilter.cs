namespace OrderBot.MessageProcessors
{
    /// <summary>
    /// Use a fixed set of minor factions.
    /// </summary>
    internal class FixedMinorFactionNameFilter : MinorFactionNameFilter
    {
        public FixedMinorFactionNameFilter(IEnumerable<string> minorFactions)
        {
            MinorFactions = new HashSet<string>(minorFactions);
        }

        public IReadOnlySet<string> MinorFactions { get; }

        /// <inheritdoc/>
        public override bool Matches(string name)
        {
            return MinorFactions.Contains(name);
        }
    }
}
