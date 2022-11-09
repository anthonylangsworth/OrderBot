using OrderBot.Core;

namespace OrderBot.Admin
{
    public interface IDiscordAuditLog
    {
        void Audit(DiscordGuild discordGuild, string message);
    }
}