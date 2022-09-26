using System.Collections.Immutable;

namespace OrderBot
{
    // Must be public for tests
    public class MinorFactionInfo : IEquatable<MinorFactionInfo?>
    {
        public MinorFactionInfo(string minorFaction, double influence, IEnumerable<string> states)
        {
            MinorFaction = minorFaction;
            Influence = influence;
            States = states.ToImmutableSortedSet();
        }

        public string MinorFaction { get; }
        public double Influence { get; }
        public IImmutableSet<string> States { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as MinorFactionInfo);
        }

        public bool Equals(MinorFactionInfo? other)
        {
            return other != null &&
                   MinorFaction == other.MinorFaction &&
                   Influence == other.Influence &&
                   States.SetEquals(other.States);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MinorFaction, Influence, States);
        }

        public override string ToString()
        {
            return $"MinorFaction = {MinorFaction}, Influence = {Influence}, States = {string.Join(',', States)}";
        }
    }
}
