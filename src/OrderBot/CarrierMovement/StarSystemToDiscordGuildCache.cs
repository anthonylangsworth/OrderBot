using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Cache the mapping of which discord guilds are monitoring which star systems.
/// </summary>
internal class StarSystemToDiscordGuildCache : MessageProcessingCache
{
    /// <summary>
    /// Create a new <see cref="StarSystemToDiscordGuildCache"/>.
    /// </summary>
    /// <param name="memoryCache">
    /// Used for caching.
    /// </param>
    public StarSystemToDiscordGuildCache(IMemoryCache memoryCache)
        : base(memoryCache, nameof(StarSystemToDiscordGuildCache))
    {
        // Do nothing
    }

    /// <summary>
    /// Get the Discord Guild Ids for this system.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="starSystemName">
    /// The star system to check.
    /// </param>
    /// <returns>
    /// The list.
    /// </returns>
    public IReadOnlySet<ulong> GetGuildsForStarSystem(OrderBotDbContext dbContext, string starSystemName)
    {
        IDictionary<string, IReadOnlySet<ulong>> systemToGuild = GetSystemToGuild(dbContext);
        systemToGuild.TryGetValue(starSystemName, out IReadOnlySet<ulong>? guilds);
        return guilds ?? new HashSet<ulong>();
    }

    /// <summary>
    /// Is this star system being monitored for carrier jumps by any guild?
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="starSystemName">
    /// The star system to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if it is a monitored system, <c>false</c> otherwise.
    /// </returns>
    public bool IsMonitoredStarSystem(OrderBotDbContext dbContext, string starSystemName)
    {
        IDictionary<string, IReadOnlySet<ulong>> systemToGuild = GetSystemToGuild(dbContext);
        return systemToGuild.ContainsKey(starSystemName);
    }

    private IDictionary<string, IReadOnlySet<ulong>> GetSystemToGuild(OrderBotDbContext dbContext)
    {
        return MemoryCache.GetOrCreate(
            CacheEntryName,
            ce =>
            {
                ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
                Dictionary<string, List<ulong>> result = new();
                IEnumerable<(string Name, ulong Id)> fromGoals =
                    dbContext.DiscordGuildPresenceGoals.Include(dgpg => dgpg.DiscordGuild)
                                                        .Include(dgpg => dgpg.Presence)
                                                        .Include(dgpg => dgpg.Presence.StarSystem)
                                                        .ToList()
                                                        .Select(dgpg => (dgpg.Presence.StarSystem.Name, dgpg.DiscordGuild.GuildId));
                IEnumerable<(string, ulong)> fromPresences =
                    dbContext.Presences.Include(p => p.MinorFaction)
                                        .Include(p => p.MinorFaction.SupportedBy)
                                        .Include(p => p.StarSystem)
                                        .ToList()
                                        .SelectMany(p => p.MinorFaction.SupportedBy.Select(dg => (p.StarSystem.Name, dg.GuildId)));
                return Enumerable.Concat(fromGoals, fromPresences)
                                    .GroupBy(t => t.Item1, t => t.Item2)
                                    .ToDictionary(g => g.Key, g => (IReadOnlySet<ulong>)new HashSet<ulong>(g));
            });
    }
}
