using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderBot.EntityFramework;

namespace OrderBot.CarrierMovement;

/// <summary>
/// A cache of information used for message processing. The goal is to reduce database load and network traffic
/// for a small increase in memory and possible incorrect results for <see cref="CacheDuration"/>.
/// </summary>
/// <remarks>
/// Override this for any new cache classes, then provide one or more members to access cached data.
/// Use <see cref="MemoryCache.GetOrCreate"/> to cache data for <see cref="CacheDuration"/> using
/// the key <see cref="CacheEntryName"/>.
/// </remarks>
internal abstract class MessageProcessingCache
{
    /// <summary>
    /// Create a new <see cref="MessageProcessingCache"/>.
    /// </summary>
    /// <param name="dbContextFactory">
    /// The <see cref="IDbContextFactory{OrderBotDbContext}"/> used to create contexts
    /// to access the database.
    /// </param>
    /// <param name="memoryCache">
    /// Used for caching.
    /// </param>
    /// <param name="cacheEntryName">
    /// The unique name for the cache entry. Usually the cache's class name.
    /// </param>
    protected MessageProcessingCache(IDbContextFactory<OrderBotDbContext> dbContextFactory,
        IMemoryCache memoryCache, string cacheEntryName)
    {
        DbContextFactory = dbContextFactory;
        MemoryCache = memoryCache;
        CacheEntryName = cacheEntryName;
    }

    public static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    protected string CacheEntryName { get; }
    protected IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }
    protected IMemoryCache MemoryCache { get; }

    /// <summary>
    /// Invalidate any cached data.
    /// </summary>
    public void Invalidate()
    {
        MemoryCache.Remove(CacheEntryName);
    }
}
