using OrderBot.Core;

namespace OrderBot.Admin
{
    public interface IDiscordAuditLog : IDisposable
    {
        void Audit(DiscordGuild discordGuild, string message);
    }
}