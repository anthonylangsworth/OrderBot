using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace OrderBot.Discord;


public class ResponseFactory
{
    public EphemeralResponse GetResponse(SocketInteractionContext context, IAuditLogger auditLogger, ILogger logger)
    {
        return new EphemeralResponse(context, auditLogger, logger);
    }
}
