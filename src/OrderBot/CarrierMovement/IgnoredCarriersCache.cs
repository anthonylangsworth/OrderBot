using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderBot.EntityFramework;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Cache ignored carriers for each Discord guild.
/// </summary>
internal class IgnoredCarriersCache : MessageProcessingCache
{
    /// <summary>
    /// Create a new <see cref="IgnoredCarriersCache"/>.
    /// </summary>
    /// <param name="dbContextFactory">
    /// The <see cref="IDbContextFactory{OrderBotDbContext}"/> used to create contexts
    /// to access the database.
    /// </param>
    /// <param name="memoryCache">
    /// Used for caching.
    /// </param>
    public IgnoredCarriersCache(IDbContextFactory<OrderBotDbContext> dbContextFactory, IMemoryCache memoryCache)
        : base(dbContextFactory, memoryCache, nameof(IgnoredCarriersCache))
    {
        // Do nothing
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="discordGuildId"></param>
    /// <param name="carrierSerialNumber"></param>
    /// <returns></returns>
    public bool IsIgnored(ulong discordGuildId, string carrierSerialNumber)
    {
        IDictionary<ulong, List<string>> discordGuildToIgnoredCarrierSerialNumber =
            MemoryCache.GetOrCreate(
                CacheEntryName,
                ce =>
                {
                    ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
                    return GetIgnoredCarriers();
                });
        discordGuildToIgnoredCarrierSerialNumber.TryGetValue(discordGuildId, out List<string>? ignoredCarriers);
        return ignoredCarriers != null && ignoredCarriers.Contains(carrierSerialNumber);
    }

    private IDictionary<ulong, List<string>> GetIgnoredCarriers()
    {
        using OrderBotDbContext dbContext = DbContextFactory.CreateDbContext();
        return dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                      .ToDictionary(dg => dg.GuildId, dg => dg.IgnoredCarriers.Select(ic => ic.SerialNumber).ToList());
    }
}

