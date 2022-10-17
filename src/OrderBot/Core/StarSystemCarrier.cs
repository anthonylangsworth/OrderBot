namespace OrderBot.Core
{
    public record StarSystemCarrier
    {
        public int Id { get; }
        public StarSystem StarSystem { get; init; } = null!;
        public Carrier Carrier { get; init; } = null!;
        public DateTime FirstSeen { get; init; }
    }
}
