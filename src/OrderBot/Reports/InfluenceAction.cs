using OrderBot.Core;

namespace OrderBot.Reports
{
    internal record InfluenceAction
    {
        public StarSystem StarSystem { get; init; } = null!;
        public double Influence { get; set; }
    }
}
