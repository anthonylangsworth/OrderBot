namespace OrderBot.Core
{
    public record DiscordGuild
    {
        public int Id { init; get; }
        public string Showflake { init; get; } = null!;
    }
}
