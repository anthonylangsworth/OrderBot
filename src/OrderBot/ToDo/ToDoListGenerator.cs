using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
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

            IReadOnlyList<StarSystemMinorFaction> systemBgsData =
                dbContext.StarSystemMinorFactions.Include(ssmf => ssmf.MinorFaction)
                                                 .Include(ssmf => ssmf.StarSystem)
                                                 .ToList();

            IReadOnlyList<DiscordGuildStarSystemMinorFactionGoal> dgssmfgs =
                dbContext.DiscordGuildStarSystemMinorFactionGoals.Include(dgssmf => dgssmf.DiscordGuild)
                                                                 .Include(dgssmf => dgssmf.StarSystemMinorFaction.StarSystem)
                                                                 .Include(dgssmf => dgssmf.StarSystemMinorFaction.MinorFaction)
                                                                 .Where(dgssmf => dgssmf.DiscordGuild.GuildId == guildId
                                                                               && dgssmf.StarSystemMinorFaction.MinorFaction.Name == minorFactionName)
                                                                 .ToList();

            // TODO: Consider Aggregate()

            foreach (DiscordGuildStarSystemMinorFactionGoal dgssmfg in dgssmfgs)
            {
                if (!Goals.Map.TryGetValue(dgssmfg.Goal, out Goal? goal))
                {
                    Logger.LogError("Skipping unknown goal '{goal}' for star system '{starSystem}' for minor faction '{minorFaction}'",
                        dgssmfg.Goal, dgssmfg.StarSystemMinorFaction.StarSystem.Name, dgssmfg.StarSystemMinorFaction.MinorFaction.Name);
                }
                else
                {
                    goal.AddActions(dgssmfg.StarSystemMinorFaction, systemBgsData, toDoList);
                }
            }

            IReadOnlyList<StarSystemMinorFaction> filtered =
                systemBgsData.Where(ssmf => !dgssmfgs.Select(dgssmfg => dgssmfg.StarSystemMinorFaction.Id).Contains(ssmf.Id)
                                                                        && ssmf.MinorFaction.Name == minorFactionName)
                             .ToList();
            foreach (StarSystemMinorFaction ssmf in filtered)
            {
                Goals.Default.AddActions(ssmf, systemBgsData, toDoList);
            }

            return toDoList;
        }
    }
}
