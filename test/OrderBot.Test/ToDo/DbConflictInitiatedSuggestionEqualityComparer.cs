using OrderBot.ToDo;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test.ToDo;

/// <summary>
/// Compare two <see cref="ConflictSuggestion"/>s, ignoring database-induced 
/// differences with <see cref="DateTime"/>s.
/// </summary>
internal class DbConflictInitiatedSuggestionEqualityComparer : IEqualityComparer<ConflictSuggestion?>
{
    public static readonly DbConflictInitiatedSuggestionEqualityComparer Instance = new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    protected DbConflictInitiatedSuggestionEqualityComparer()
    {
        // Do nothing
    }

    public bool Equals(ConflictSuggestion? x, ConflictSuggestion? y)
    {
        // Comparig MinorFaction.SupportedBy is hazardous

        return x is not null &&
               y is not null &&
               x.StarSystem.Id == y.StarSystem.Id &&
               x.StarSystem.Name == y.StarSystem.Name &&
               DbDateTimeComparer.Instance.Equals(x.StarSystem.LastUpdated, y.StarSystem.LastUpdated) &&
               x.FightFor.Id == y.FightFor.Id &&
               x.FightFor.Name == y.FightFor.Name &&
               // x.FightFor.SupportedBy.SetEquals(y.FightFor.SupportedBy) &&
               x.FightForWonDays == y.FightForWonDays &&
               x.FightAgainst.Id == y.FightAgainst.Id &&
               x.FightAgainst.Name == y.FightAgainst.Name &&
               // x.FightAgainst.SupportedBy.SetEquals(y.FightAgainst.SupportedBy) &&
               x.FightAgainstWonDays == y.FightAgainstWonDays &&
               x.State == y.State &&
               x.WarType == y.WarType;
    }

    public int GetHashCode([DisallowNull] ConflictSuggestion obj)
    {
        throw new NotImplementedException();
    }
}
