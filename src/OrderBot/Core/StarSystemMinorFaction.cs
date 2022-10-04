namespace OrderBot.Core
{
    public record StarSystemMinorFaction
    {
        public int Id { get; }
        public StarSystem StarSystem { get; init; } = null!;
        public MinorFaction MinorFaction { get; init; } = null!;
        public double Influence;
        public List<State> States { get; } = new();
    }
}
