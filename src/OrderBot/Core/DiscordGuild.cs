namespace OrderBot.Core
{
    public record DiscordGuild
    {
        public int Id { init; get; }
        public long GuildId { init; get; }
        public long? CarrierMovementChannel { set; get; }
        public ICollection<Carrier> IgnoredCarriers { init; get; } = null!;
    }
}
