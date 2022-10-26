using OrderBot.Core;

namespace OrderBot.ToDo
{
    public record SecurityInitiatedSuggestion : IEquatable<SecurityInitiatedSuggestion?>
    {
        public StarSystem StarSystem { get; init; } = null!;
        public string? SecurityLevel { get; set; } = null;
        public string? Description { get; set; } = null;
    }
}
