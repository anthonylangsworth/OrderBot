namespace OrderBot.Core
{
    public record StarSystem
    {
        public int Id { get; }
        public string Name { get; init; } = null!;
        public DateTime? LastUpdated { get; set; }
    }
}
