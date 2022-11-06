using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.ToDo
{
    public class ToDoListGenerator
    {
        public ToDoListGenerator(ILogger<ToDoListGenerator> logger,
            IDbContextFactory<OrderBotDbContext> dbContextFactory)
        {
            Logger = logger;
            DbContextFactory = dbContextFactory;
        }

        public ILogger<ToDoListGenerator> Logger { get; }
        public IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }

        public ToDoList Generate(ulong guildId, string minorFactionName)
        {
            ToDoList toDoList = new(minorFactionName);

            using OrderBotDbContext dbContext = DbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            IReadOnlyList<Presence> bgsData =
                dbContext.Presences.Include(ssmf => ssmf.MinorFaction)
                                                 .Include(ssmf => ssmf.StarSystem)
                                                 .ToList();

            IReadOnlyList<DiscordGuildPresenceGoal> dgssmfgs =
                dbContext.DiscordGuildPresenceGoals.Include(dgssmf => dgssmf.DiscordGuild)
                                                                 .Include(dgssmf => dgssmf.Presence.StarSystem)
                                                                 .Include(dgssmf => dgssmf.Presence.MinorFaction)
                                                                 .Where(dgssmf => dgssmf.DiscordGuild.GuildId == guildId
                                                                               && dgssmf.Presence.MinorFaction.Name == minorFactionName)
                                                                 .ToList();

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
                    Logger.LogError("Skipping unknown goal '{goal}' for star system '{starSystem}' for minor faction '{minorFaction}'",
                        dgssmfg.Goal, dgssmfg.Presence.StarSystem.Name, dgssmfg.Presence.MinorFaction.Name);
                }
                else
                {
                    toDoList.Suggestions.UnionWith(
                        goal.GetSuggestions(dgssmfg.Presence, starSystemBgsData, conflicts));
                }
            }

            IReadOnlyList<Presence> filtered =
                bgsData.Where(ssmf => !dgssmfgs.Select(dgssmfg => dgssmfg.Presence.Id).Contains(ssmf.Id)
                                                                        && ssmf.MinorFaction.Name == minorFactionName)
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
}
