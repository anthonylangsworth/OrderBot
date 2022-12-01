using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace OrderBot.Discord;


public class ResultFactory
{
    public EphemeralResult GetResponse(SocketInteractionContext context, IAuditLogger auditLogger, ILogger logger)
    {
        return new EphemeralResult(context, auditLogger, logger);
    }
}
