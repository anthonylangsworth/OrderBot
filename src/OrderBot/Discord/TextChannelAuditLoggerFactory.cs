using Discord.Interactions;
using OrderBot.EntityFramework;

namespace OrderBot.Discord;

/// <summary>
/// A factory class to produce <see cref="TextChannelAuditLogger"/> objects.
/// </summary>
public class TextChannelAuditLoggerFactory
{
    /// <summary>
    /// Create a new <see cref="TextChannelAuditLogger"/>.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> used to access
    /// database stored configuration.
    /// </param>
    public TextChannelAuditLoggerFactory(OrderBotDbContext dbContext)
    {
        DbContext = dbContext;
    }

    /// <summary>
    /// Used for database-stored configuation.
    /// </summary>
    internal OrderBotDbContext DbContext { get; }

    /// <summary>
    /// Create a <see cref="IAuditLogger"/>.
    /// </summary>
    /// <param name="context">
    /// Details of the discord interaction.
    /// </param>
    /// <returns>
    /// An appropriate <see cref="IAuditLogger"/>.
    /// </returns>
    public IAuditLogger CreateAuditLogger(SocketInteractionContext context)
    {
        ulong auditChannelId = GetAuditChannel(context);
        return auditChannelId != 0
            ? new TextChannelAuditLogger(context, auditChannelId)
            : new NullAuditLogger();
    }

    /// <summary>
    /// Return the channel ID of the audit channel for the current discord guild.
    /// </summary>
    /// <param name="context">
    /// The <see cref="SocketInteractionContext"/> for the current discord interaction.
    /// </param>
    /// <returns>
    /// The channel ID or o, if there is no configured channel ID.
    /// </returns>
    protected internal ulong GetAuditChannel(SocketInteractionContext context)
    {
        // TODO: Add caching ? E.g. MemoryCache memoryCache = new MemoryCache();
        return DbContext.DiscordGuilds.FirstOrDefault(dg => dg.GuildId == context.Guild.Id)?.AuditChannel ?? 0;
    }
}
