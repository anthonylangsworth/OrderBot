using Microsoft.Extensions.Caching.Memory;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Cache supported minor factions across all discord guilds.
/// </summary>
public class SupportedMinorFactionsCache : MessageProcessorCache
{
    /// <summary>
    /// Create a new <see cref="SupportedMinorFactionsCache"/>.
    /// </summary>
    /// <param name="memoryCache">
    /// Used for caching.
    /// </param>
    public SupportedMinorFactionsCache(IMemoryCache memoryCache)
        : base(memoryCache, nameof(SupportedMinorFactionsCache))
    {
        // Do nothing
    }

    /// <summary>
    /// Is the minor faction <paramref name="minorFactionName"/> supported
    /// by one or more discord guilds?
    /// </summary>
    /// <param name="dbContext">
    /// The database to use.
    /// </param>
    /// <param name="minorFactionName">
    /// The name of the minor faction to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if the minor faction is supported by one or more discord guilds,
    /// or <c>false</c> if not.
    /// </returns>
    public bool IsSupported(OrderBotDbContext dbContext, string minorFactionName)
    {
        IReadOnlySet<string> supportedMinorFactions =
            MemoryCache.GetOrCreate(
                CacheEntryName,
                ce =>
                {
                    ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
                    return GetSupportedMinorFactions(dbContext);
                });
        return supportedMinorFactions.Contains(minorFactionName);
    }

    /// <summary>
    /// Get the supported minor factions across all Discord guilds. Details from these star systems should be processed.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns>
    /// The supported minor factions.
    /// </returns>
    private static IReadOnlySet<string> GetSupportedMinorFactions(OrderBotDbContext dbContext)
    {
        return dbContext.DiscordGuildMinorFactions.Select(dgmf => dgmf.MinorFaction.Name).Distinct().ToHashSet();
    }
}

