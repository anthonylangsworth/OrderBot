namespace OrderBot.Core;

public record DiscordGuild
{
    public int Id { init; get; }
    public ulong GuildId { init; get; }
    public string? Name { set; get; }
    public ulong? CarrierMovementChannel { set; get; }
    public ulong? AuditChannel { set; get; }
    public ICollection<Carrier> IgnoredCarriers { init; get; } = new List<Carrier>();
    public ICollection<MinorFaction> SupportedMinorFactions { init; get; } = new List<MinorFaction>();
}
