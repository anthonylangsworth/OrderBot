namespace OrderBot.ToDo
{
    internal record EddnStarSystemData
    {
        public DateTime Timestamp { init; get; }
        public string StarSystemName { init; get; } = null!;
        public IReadOnlyList<EddnMinorFactionInfluence> MinorFactionDetails { init; get; } = null!;
        public string SystemSecurityLevel { init; get; } = null!;
        public IReadOnlyList<EddnConflict> Conflicts { init; get; } = null!;
    }
}
