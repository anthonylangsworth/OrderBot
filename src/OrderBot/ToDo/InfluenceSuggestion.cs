using OrderBot.Core;

namespace OrderBot.ToDo;

public record InfluenceSuggestion(StarSystem StarSystem, MinorFaction MinorFaction,
    bool Pro, double Influence = 0, string? Description = null)
    : Suggestion(StarSystem, Description), IEquatable<InfluenceSuggestion?>
{
}
