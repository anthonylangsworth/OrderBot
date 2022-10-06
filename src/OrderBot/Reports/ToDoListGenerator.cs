using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using System.Transactions;

namespace OrderBot.Reports
{
    internal class ToDoListGenerator
    {
        public ToDoListGenerator(ILogger<ToDoListGenerator> logger,
            IDbContextFactory<OrderBotDbContext> dbContextFactory)
        {
            Logger = logger;
            DbContextFactory = dbContextFactory;
        }

        public ILogger<ToDoListGenerator> Logger { get; }
        public IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }

        public ToDoList Generate(string guildId, string minorFactionName)
        {
            ToDoList toDoList = new(minorFactionName);

            using OrderBotDbContext dbContext = DbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            IQueryable<DiscordGuildStarSystemMinorFactionGoal> dgssmfgs =
                dbContext.DiscordGuildStarSystemMinorFactionGoals.Include(dgssmf => dgssmf.DiscordGuild)
                                                                 .Include(dgssmf => dgssmf.StarSystemMinorFaction)
                                                                 .Include(dgssmf => dgssmf.StarSystemMinorFaction.MinorFaction)
                                                                 .Include(dgssmf => dgssmf.StarSystemMinorFaction.StarSystem)
                                                                 .Where(dgssmf => dgssmf.DiscordGuild.GuildId == guildId
                                                                               && dgssmf.StarSystemMinorFaction.MinorFaction.Name == minorFactionName);

            // TODO: Consider Aggregate()

            foreach (DiscordGuildStarSystemMinorFactionGoal dgssmfg in dgssmfgs)
            {
                Goal? goal;
                if (dgssmfg.Goal == null)
                {
                    goal = Goals.Default;
                }
                else if (!Goals.Map.TryGetValue(dgssmfg.Goal, out goal))
                {
                    Logger.LogError("Skipping unknown goal '{goal}' for star system '{starSystem}' for minor faction '{minorFaction}'",
                        dgssmfg.Goal, dgssmfg.StarSystemMinorFaction.StarSystem.Name, dgssmfg.StarSystemMinorFaction.MinorFaction.Name);
                }

                if (goal != null)
                {
                    goal.AddActions(dgssmfg.StarSystemMinorFaction, toDoList);
                }
            }

            return toDoList;
        }
    }
}
