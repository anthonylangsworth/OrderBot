namespace OrderBot.Core
{
    public record Role
    {
        public int Id { get; }
        public string Name { get; init; } = null!;
    }
}
