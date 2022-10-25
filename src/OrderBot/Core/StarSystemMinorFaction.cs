namespace OrderBot.Core
{
    public record StarSystemMinorFaction
    {
        public int Id { get; }
        public StarSystem StarSystem { get; init; } = null!;
        public MinorFaction MinorFaction { get; init; } = null!;
        public double Influence;
        public string? Security;
        public List<State> States { get; } = new();
    }
}
