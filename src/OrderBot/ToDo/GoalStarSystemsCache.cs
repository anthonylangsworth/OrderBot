using Microsoft.Extensions.Caching.Memory;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Cache systems with at least one goal.
/// </summary>
public class GoalStarSystemsCache : MessageProcessorCache
{
    /// <summary>
    /// Create a new <see cref="GoalStarSystemsCache"/>.
    /// </summary>
    /// <param name="memoryCache">
    /// Used for caching.
    /// </param>
    public GoalStarSystemsCache(IMemoryCache memoryCache)
        : base(memoryCache, nameof(GoalStarSystemsCache))
    {
        // Do nothing
    }

    /// <summary>
    /// Does the <paramref name="starSystemName"/> have one or more goals?
    /// </summary>
    /// <param name="dbContext">
    /// The database to use.
    /// </param>
    /// <param name="starSystemName">
    /// The name of the star system to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if <paramref name="starSystemName"/> has one or more goals,
    /// or <c>false</c> if not.
    /// </returns>
    public bool HasGoal(OrderBotDbContext dbContext, string starSystemName)
    {
        IReadOnlySet<string> goalStarSystems =
            MemoryCache.GetOrCreate(
                CacheEntryName,
                ce =>
                {
                    ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
                    return GetGoalSystems(dbContext);
                });
        return goalStarSystems.Contains(starSystemName);
    }

    /// <summary>
    /// Get the star systems used with goals across all Discord Guilds. Details from these star systems should be processed.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <returns>
    /// The star systems associated with <see cref="Goal"/>s.
    /// </returns>
    internal static IReadOnlySet<string> GetGoalSystems(OrderBotDbContext dbContext)
    {
        return dbContext.DiscordGuildPresenceGoals.Select(dgpg => dgpg.Presence.StarSystem.Name).Distinct().ToHashSet();
    }
}

