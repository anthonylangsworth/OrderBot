using OrderBot.ToDo;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test.ToDo;

internal class EddnConflictEqualityComparer : IEqualityComparer<EddnConflict>
{
    public static readonly EddnConflictEqualityComparer Instance = new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    protected EddnConflictEqualityComparer()
    {
        // Do nothing
    }

    public bool Equals(EddnConflict? x, EddnConflict? y)
    {
        return x is not null
            && y is not null
            && x.Faction1.Name == y.Faction1.Name
            && x.Faction1.Stake == y.Faction1.Stake
            && x.Faction1.WonDays == y.Faction1.WonDays
            && x.Faction2.Name == y.Faction2.Name
            && x.Faction2.Stake == y.Faction2.Stake
            && x.Faction2.WonDays == y.Faction2.WonDays
            && x.WarType == y.WarType
            && x.Status == y.Status;
    }

    public int GetHashCode([DisallowNull] EddnConflict obj)
    {
        throw new NotImplementedException();
    }
}
