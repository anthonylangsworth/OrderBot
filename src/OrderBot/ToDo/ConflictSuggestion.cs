using OrderBot.Core;

namespace OrderBot.ToDo;

public record ConflictSuggestion(StarSystem StarSystem, MinorFaction FightFor, int FightForWonDays,
    MinorFaction FightAgainst, int FightAgainstWonDays, string State, string WarType, string? Description = null)
    : Suggestion(StarSystem, Description), IEquatable<ConflictSuggestion?>
{
}
