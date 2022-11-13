namespace OrderBot.Core;

public record MinorFaction
{
    public int Id { init; get; }
    public string Name { init; get; } = null!;
    public HashSet<DiscordGuild> SupportedBy { init; get; } = null!;
}
