namespace OrderBot.Core
{
    public record DiscordGuildStarSystemMinorFactionGoal
    {
        public int Id { get; init; }
        public DiscordGuild DiscordGuild { get; set; } = null!;
        public StarSystemMinorFaction StarSystemMinorFaction { get; set; } = null!;
        public string? Goal { get; set; } = null;
    }
}
