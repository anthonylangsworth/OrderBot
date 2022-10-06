using OrderBot.Core;

namespace OrderBot.Reports
{
    public record InfluenceInitiatedAction : IEquatable<InfluenceInitiatedAction?>
    {
        public StarSystem StarSystem { get; init; } = null!;
        public double Influence { get; set; }
    }
}
