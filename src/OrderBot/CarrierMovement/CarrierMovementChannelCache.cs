using Microsoft.Extensions.Caching.Memory;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Cache carrier movement channel IDs  for each Discord guild.
/// Called by <see cref="EddnMessageHostedService"/>.
/// </summary>
public class CarrierMovementChannelCache : MessageProcessorCache
{
    /// <summary>
    /// Create a new <see cref="CarrierMovementChannelCache"/>.
    /// </summary>
    /// <param name="memoryCache">
    /// Used for caching.
    /// </param>
    public CarrierMovementChannelCache(IMemoryCache memoryCache)
        : base(memoryCache, nameof(CarrierMovementChannelCache))
    {
        // Do nothing
    }

    /// <summary>
    /// Get the carrier movement channel, if any, for the specified Discord guild.
    /// </summary>
    /// <param name="dbContext">
    /// The database to use.
    /// </param>
    /// <param name="discordGuildId">
    /// The ID of a <see cref="DiscordGuild"/>.
    /// </param>    
    /// <returns>
    /// The ID of the carrier movement channel or <c>null</c>, if none is configured.
    /// </returns>
    public ulong? GetCarrierMovementChannel(OrderBotDbContext dbContext, ulong discordGuidId)
    {
        IDictionary<ulong, ulong?> discordGuildToCarrierMovementChannel =
            MemoryCache.GetOrCreate(
                CacheEntryName,
                ce =>
                {
                    ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
                    return GetCarrierMovementChannel(dbContext);
                }) ?? new();
        discordGuildToCarrierMovementChannel.TryGetValue(discordGuidId, out ulong? carrierMovementChannelId);
        return carrierMovementChannelId;
    }

    private static Dictionary<ulong, ulong?> GetCarrierMovementChannel(OrderBotDbContext dbContext)
    {
        return dbContext.DiscordGuilds.ToDictionary(dg => dg.GuildId, dg => dg.CarrierMovementChannel);
    }
}
