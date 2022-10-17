namespace OrderBot.Core
{
    public record DiscordGuildMinorFaction
    {
        public int Id { get; }
        public DiscordGuild DiscordGuild { get; init; } = null!;
        public MinorFaction MinorFaction { get; init; } = null!;
    }
}
