using OrderBot.Core;

namespace OrderBot.ToDo
{
    public record ConflictSuggestion : Suggestion, IEquatable<ConflictSuggestion?>
    {
        public MinorFaction MinorFaction1 { get; init; } = null!;
        public int MinorFaction1WonDays { get; init; }
        public MinorFaction MinorFaction2 { get; init; } = null!;
        public int MinorFaction2WonDays { get; init; }
        public string State { get; init; } = null!;
        public MinorFaction FightFor { get; init; } = null!;
    }
}
