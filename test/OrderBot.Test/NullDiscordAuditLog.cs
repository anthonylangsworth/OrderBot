using OrderBot.Admin;
using OrderBot.Core;

namespace OrderBot.Test
{
    internal class NullDiscordAuditLog : IDiscordAuditLog
    {
        public void Audit(DiscordGuild discordGuild, string message)
        {
            // Do nothing
        }

        public void Dispose()
        {
            // Do nothing
        }
    }
}
