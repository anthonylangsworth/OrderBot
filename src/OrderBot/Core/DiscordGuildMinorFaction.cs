namespace OrderBot.Core;

public class DiscordGuildMinorFaction
{
    public int Id { get; }
    public DiscordGuild DiscordGuild { get; init; } = null!;
    public MinorFaction MinorFaction { get; init; } = null!;

    public override string ToString()
    {
        return $"{DiscordGuild} supports {MinorFaction}";
    }
}
