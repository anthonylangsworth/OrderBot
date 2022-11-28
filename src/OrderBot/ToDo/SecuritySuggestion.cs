using OrderBot.Core;

namespace OrderBot.ToDo;

public record SecuritySuggestion(StarSystem StarSystem, string SecurityLevel, string? Description = null)
    : Suggestion(StarSystem, Description), IEquatable<SecuritySuggestion?>
{
}
