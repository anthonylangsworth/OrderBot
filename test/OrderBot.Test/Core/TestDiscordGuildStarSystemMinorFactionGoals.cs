using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;
using System.Transactions;

namespace OrderBot.Test.Core
{
    internal class TestDiscordGuildStarSystemMinorFactionGoals
    {
        [Test]
        [Ignore("Only run manually")]
        [TestCase("Gally Bese", "EDA Kunti League", "Ignore")]
        [TestCase("Sanka", "EDA Kunti League", "Ignore")]
        [TestCase("Marya Wang", "EDA Kunti League", "Ignore")]
        [TestCase("Lutni", "EDA Kunti League", "Ignore")]
        [TestCase("CPD-59 314", "EDA Kunti League", "Ignore")]
        [TestCase("San Davokje", "EDA Kunti League", "Ignore")]
        [TestCase("HR 2283", "EDA Kunti League", "Maintain")]
        [TestCase("LTT 2337", "EDA Kunti League", "Maintain")]
        [TestCase("Eta-1 Pictoris", "EDA Kunti League", "Maintain")]
        [TestCase("Kunti", "LTT 2337 Empire Party", "Maintain")]
        public void SetupEDASystems(string starSystemName, string minorFactionName, string goalName)
        {
            ulong edaGuildId = 141831692699566080;
            using OrderBotDbContextFactory orderBotDbContextFactory = new();
            using OrderBotDbContext dbContext = orderBotDbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            DiscordGuild discordGuild = dbContext.DiscordGuilds.First(dg => dg.GuildId == edaGuildId);
            StarSystem starSystem = dbContext.StarSystems.First(ss => ss.Name == starSystemName);
            MinorFaction minorFaction = dbContext.MinorFactions.First(mf => mf.Name == minorFactionName);
            Assert.IsTrue(Goals.Map.TryGetValue(goalName, out OrderBot.ToDo.Goal? goal));

            DiscordGuildStarSystemMinorFactionGoal? discordGuildStarSystemMinorFactionGoal =
                dbContext.DiscordGuildStarSystemMinorFactionGoals
                         .Include(dgssmfg => dgssmfg.StarSystemMinorFaction)
                         .Include(dgssmfg => dgssmfg.StarSystemMinorFaction.StarSystem)
                         .Include(dgssmfg => dgssmfg.StarSystemMinorFaction.MinorFaction)
                         .FirstOrDefault(
                            dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                     && dgssmfg.StarSystemMinorFaction.MinorFaction == minorFaction
                                     && dgssmfg.StarSystemMinorFaction.StarSystem == starSystem);
            if (discordGuildStarSystemMinorFactionGoal == null)
            {
                StarSystemMinorFaction starSystemMinorFaction =
                    dbContext.StarSystemMinorFactions.First(
                        ssmf => ssmf.MinorFaction == minorFaction
                              && ssmf.StarSystem == starSystem);
                if (starSystemMinorFaction == null)
                {
                    starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction };
                    dbContext.StarSystemMinorFactions.Add(starSystemMinorFaction);
                }

                discordGuildStarSystemMinorFactionGoal = new()
                {
                    DiscordGuild = discordGuild,
                    StarSystemMinorFaction = starSystemMinorFaction
                };
                dbContext.DiscordGuildStarSystemMinorFactionGoals.Add(discordGuildStarSystemMinorFactionGoal);
            }
            discordGuildStarSystemMinorFactionGoal.Goal = goalName;
            dbContext.SaveChanges();
            transactionScope.Complete();
        }
    }
}
