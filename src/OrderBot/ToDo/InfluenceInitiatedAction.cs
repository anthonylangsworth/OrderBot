using OrderBot.Core;

namespace OrderBot.ToDo
{
    public record InfluenceInitiatedAction : IEquatable<InfluenceInitiatedAction?>
    {
        public StarSystem StarSystem { get; init; } = null!;
        public double Influence { get; set; }
        public string? Description { get; set; } = null;
    }
}
