using Microsoft.EntityFrameworkCore;
using OrderBot.Core;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.ToDo;

/// <summary>
/// Generate a <see cref="ToDoList"/>.
/// </summary>
public class ToDoListGenerator
{
    public ToDoListGenerator(IDbContextFactory<OrderBotDbContext> dbContextFactory)
    {
        DbContextFactory = dbContextFactory;
    }

    public IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }

    /// <summary>
    /// Generate a <see cref="ToDoList"/>.
    /// </summary>
    /// <param name="guildId">
    /// The guild ID to generate the suggestions for.
    /// </param>
    /// <returns>
    /// The generated <see cref="ToDoList"/>.
    /// </returns>
    /// <exception cref="UnknownGoalException">
    /// An unknown <see cref="Goal"/> is used for a star system and minor faction.
    /// </exception>
    /// <exception cref="NoSupportedMinorFactionException">
    /// 
    /// </exception>
    public ToDoList Generate(ulong guildId)
    {
        using OrderBotDbContext dbContext = DbContextFactory.CreateDbContext();
        using TransactionScope transactionScope = new();

        string supportedMinorFactionName;
        try
        {
            supportedMinorFactionName =
                dbContext.DiscordGuilds.Include(dg => dg.SupportedMinorFactions)
                                       .First(dg => dg.GuildId == guildId)
                                       .SupportedMinorFactions.First().Name;
        }
        catch (InvalidOperationException ex)
        {
            throw new NoSupportedMinorFactionException(guildId, ex);
        }

        ToDoList toDoList = new(supportedMinorFactionName);

        IReadOnlyList<Presence> bgsData =
            dbContext.Presences.Include(ssmf => ssmf.MinorFaction)
                                             .Include(ssmf => ssmf.StarSystem)
                                             .ToList();

        IReadOnlyList<DiscordGuildPresenceGoal> dgssmfgs =
            dbContext.DiscordGuildPresenceGoals.Include(dgssmf => dgssmf.DiscordGuild)
                                                             .Include(dgssmf => dgssmf.Presence.StarSystem)
                                                             .Include(dgssmf => dgssmf.Presence.MinorFaction)
                                                             .Where(dgssmf => dgssmf.DiscordGuild.GuildId == guildId
                                                                           && dgssmf.Presence.MinorFaction.Name == supportedMinorFactionName)
                                                             .ToList();

        // Handle explicit goals
        foreach (DiscordGuildPresenceGoal dgssmfg in dgssmfgs)
        {
            HashSet<Presence> starSystemBgsData =
                bgsData.Where(ssmf2 => ssmf2.StarSystem == dgssmfg.Presence.StarSystem)
                       .ToHashSet();
            HashSet<Conflict> conflicts = dbContext.Conflicts.Include(c => c.MinorFaction1)
                                                             .Include(c => c.MinorFaction2)
                                                             .Where(c => c.StarSystem == dgssmfg.Presence.StarSystem)
                                                             .ToHashSet();

            if (!Goals.Map.TryGetValue(dgssmfg.Goal, out Goal? goal))
            {
                throw new UnknownGoalException(
                    dgssmfg.Goal, dgssmfg.Presence.StarSystem.Name, dgssmfg.Presence.MinorFaction.Name);
            }
            else
            {
                toDoList.Suggestions.UnionWith(
                    goal.GetSuggestions(dgssmfg.Presence, starSystemBgsData, conflicts));
            }
        }

        // Handle remaing systems using default goal
        IReadOnlyList<Presence> filtered =
            bgsData.Where(ssmf => !dgssmfgs.Select(dgssmfg => dgssmfg.Presence.Id).Contains(ssmf.Id)
                                                           && ssmf.MinorFaction.Name == supportedMinorFactionName)
                   .ToList();
        foreach (Presence ssmf in filtered)
        {
            HashSet<Presence> starSystemBgsData =
                bgsData.Where(ssmf2 => ssmf2.StarSystem == ssmf.StarSystem)
                       .ToHashSet();
            HashSet<Conflict> conflicts = dbContext.Conflicts.Include(c => c.MinorFaction1)
                                                             .Include(c => c.MinorFaction2)
                                                             .Where(c => c.StarSystem == ssmf.StarSystem)
                                                             .ToHashSet();

            toDoList.Suggestions.UnionWith(
                Goals.Default.GetSuggestions(ssmf, starSystemBgsData, conflicts));
        }

        return toDoList;
    }
}
