namespace OrderBot.Core
{
    public class Conflict
    {
        public int Id { get; init; }
        public StarSystem StarSystem { get; init; } = null!;
        public MinorFaction MinorFaction1 { get; init; } = null!;
        public int MinorFaction1DaysWon { get; set; } = 0;
        public MinorFaction MinorFaction2 { get; init; } = null!;
        public int MinorFaction2DaysWon { get; set; } = 0;
        public string? Status { get; set; } = null;
        public string WarType { get; init; } = null!;
    }
}
