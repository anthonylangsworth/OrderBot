namespace OrderBot.Core
{
    public record DiscordGuild
    {
        public int Id { init; get; }
        public string GuildId { init; get; } = null!;
    }
}
