using Microsoft.Extensions.Caching.Memory;

namespace OrderBot.MessageProcessors;

/// <summary>
/// A cache of data used for message processing. The goal is to reduce database load and network traffic
/// for a small increase in memory and possible incorrect results for <see cref="CacheDuration"/>.
/// </summary>
/// <remarks>
/// <p>
/// Override this for any new cache classes, then provide one or more members to access cached data.
/// Use <see cref="MemoryCache.GetOrCreate"/> to cache data for <see cref="CacheDuration"/> using
/// the key <see cref="CacheEntryName"/>.
/// </p>
/// <p>
/// As an option, provide specific versions of <see cref="Invalidate"/> to invalidate part of
/// the cache, such as for a specific Discord server.
/// </p>
/// </remarks>
/// <seealso cref="EddnMessageProcessor"/>
internal abstract class MessageProcessorCache
{
    /// <summary>
    /// Create a new <see cref="MessageProcessorCache"/>.
    /// </summary>
    /// <param name="memoryCache">
    /// Used for caching.
    /// </param>
    /// <param name="cacheEntryName">
    /// The unique name for the cache entry. Usually the cache's class name.
    /// </param>
    protected MessageProcessorCache(IMemoryCache memoryCache, string cacheEntryName)
    {
        MemoryCache = memoryCache;
        CacheEntryName = cacheEntryName;
    }

    /// <summary>
    /// The default duration of cached entries.
    /// </summary>
    public static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    /// <summary>
    /// The ID of the cache entry in <see cref="MemoryCache"/>.
    /// </summary>
    protected string CacheEntryName { get; }
    /// <summary>
    /// Used to cache results.
    /// </summary>
    protected IMemoryCache MemoryCache { get; }

    /// <summary>
    /// Invalidate any cached data.
    /// </summary>
    public virtual void Invalidate()
    {
        MemoryCache.Remove(CacheEntryName);
    }
}
