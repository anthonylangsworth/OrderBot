using OrderBot.ToDo;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test.ToDo;

internal class EddnMinorFactionInfluenceEqualityComparer : IEqualityComparer<EddnMinorFactionInfluence>
{
    public static readonly EddnMinorFactionInfluenceEqualityComparer Instance = new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    protected EddnMinorFactionInfluenceEqualityComparer()
    {
        // Do nothing
    }

    public bool Equals(EddnMinorFactionInfluence? x, EddnMinorFactionInfluence? y)
    {
        return x is not null
            && y is not null
            && x.MinorFaction == y.MinorFaction
            && x.Influence == y.Influence
            && x.States.OrderBy(s => s).SequenceEqual(y.States.OrderBy(s => s));
    }

    public int GetHashCode([DisallowNull] EddnMinorFactionInfluence obj)
    {
        throw new NotImplementedException();
    }
}
