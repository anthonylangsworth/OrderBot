using OrderBot.Core;

namespace OrderBot.ToDo
{
    public record InfluenceInitiatedSuggestion : IEquatable<InfluenceInitiatedSuggestion?>
    {
        public StarSystem StarSystem { get; init; } = null!;
        public double Influence { get; set; }
        public string? Description { get; set; } = null;
    }
}
