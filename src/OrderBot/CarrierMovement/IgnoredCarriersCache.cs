using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Cache ignored carriers for each Discord guild.
/// </summary>
public class IgnoredCarriersCache : MessageProcessorCache
{
    /// <summary>
    /// Create a new <see cref="IgnoredCarriersCache"/>.
    /// </summary>
    /// <param name="memoryCache">
    /// Used for caching.
    /// </param>
    public IgnoredCarriersCache(IMemoryCache memoryCache)
        : base(memoryCache, nameof(IgnoredCarriersCache))
    {
        // Do nothing
    }

    /// <summary>
    /// Is the carrier with <paramref name="carrierSerialNumber"/> ignored 
    /// by guild <paramref name="discordId"/>?
    /// </summary>
    /// <param name="dbContext">
    /// The database to use.
    /// </param>
    /// <param name="discordGuildId">
    /// The ID of a <see cref="DiscordGuild"/>.
    /// </param>
    /// <param name="carrierSerialNumber">
    /// The serial number of the fleet carrier to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if the carrier is ignored or <c>false</c> if not ignored or
    /// <paramref name="discordGuildId"/> is not known
    /// </returns>
    public bool IsIgnored(OrderBotDbContext dbContext, ulong discordId, string carrierSerialNumber)
    {
        IDictionary<ulong, HashSet<string>> discordGuildToIgnoredCarrierSerialNumber =
            MemoryCache.GetOrCreate(
                CacheEntryName,
                ce =>
                {
                    ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
                    return GetIgnoredCarriers(dbContext);
                });
        discordGuildToIgnoredCarrierSerialNumber.TryGetValue(discordId, out HashSet<string>? ignoredCarriers);
        return ignoredCarriers != null && ignoredCarriers.Contains(carrierSerialNumber);
    }

    private IDictionary<ulong, HashSet<string>> GetIgnoredCarriers(OrderBotDbContext dbContext)
    {
        return dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                      .ToDictionary(dg => dg.GuildId, dg => dg.IgnoredCarriers.Select(ic => ic.SerialNumber).ToHashSet());
    }
}

