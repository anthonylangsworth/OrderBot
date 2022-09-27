using System.Collections.Immutable;

namespace OrderBot.MessageProcessors
{
    /// <summary>
    /// A minor faction's influence and states in a star system.
    /// </summary>
    internal record MinorFactionInfluence : IEquatable<MinorFactionInfluence?>
    {
        public MinorFactionInfluence(string minorFaction, double influence, IEnumerable<string> states)
        {
            MinorFaction = minorFaction;
            Influence = influence;
            States = states.ToImmutableSortedSet();
        }

        public string MinorFaction { get; }
        public double Influence { get; }
        public IImmutableSet<string> States { get; }
    }
}
