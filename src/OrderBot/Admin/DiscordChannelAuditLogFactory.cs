using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace OrderBot.Admin
{
    public class DiscordChannelAuditLogFactory
    {
        public DiscordChannelAuditLogFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public ILoggerFactory LoggerFactory { get; }

        public IDiscordAuditLog CreateAuditLog(SocketInteractionContext context)
        {
            return new DiscordChannelAuditLog(context,
                LoggerFactory.CreateLogger<DiscordChannelAuditLog>());
        }
    }
}
