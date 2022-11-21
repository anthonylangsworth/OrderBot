namespace OrderBot.Core;

public class RoleMember
{
    public int Id { get; }
    public DiscordGuild DiscordGuild { get; init; } = null!;
    public Role Role { get; init; } = null!;
    public ulong MentionableId { get; init; }

    public override string ToString()
    {
        return $"{MentionableId} in {Role} in {DiscordGuild}";
    }
}
