using OrderBot.Core;

namespace OrderBot.ToDo;

public record ConflictSuggestion : Suggestion, IEquatable<ConflictSuggestion?>
{
    public MinorFaction FightFor { get; init; } = null!;
    public int FightForWonDays { get; init; }
    public MinorFaction FightAgainst { get; init; } = null!;
    public int FightAgainstWonDays { get; init; }
    public string State { get; init; } = null!;
    public string WarType { get; init; } = null!;
}
