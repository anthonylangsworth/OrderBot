namespace OrderBot.ToDo
{
    internal record BgsStarSystemData
    {
        public DateTime Timestamp { init; get; }
        public string StarSystemName { init; get; } = null!;
        public IReadOnlyList<MinorFactionInfluence> MinorFactionDetails { init; get; } = null!;
        public string SystemSecurityState { init; get; } = null!;
    }
}
