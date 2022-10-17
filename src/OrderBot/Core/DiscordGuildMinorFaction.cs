namespace OrderBot.Core
{
    internal record DiscordGuildMinorFaction
    {
        public int Id { get; }
        public DiscordGuild DiscordGuild { get; init; } = null!;
        public MinorFaction MinorFaction { get; init; } = null!;
    }
}
