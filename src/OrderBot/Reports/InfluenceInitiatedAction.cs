using OrderBot.Core;

namespace OrderBot.Reports
{
    internal record InfluenceInitiatedAction : IEquatable<InfluenceInitiatedAction?>
    {
        public StarSystem StarSystem { get; init; } = null!;
        public double Influence { get; set; }
    }
}
