using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.EntityFramework;
using OrderBot.ToDo;
using System.Transactions;

namespace OrderBot.Test.Core
{
    internal class DiscordGuildStarSystemMinorFactionGoalsTests
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

            DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
                dbContext.DiscordGuildPresenceGoals
                         .Include(dgssmfg => dgssmfg.Presence)
                         .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                         .Include(dgssmfg => dgssmfg.Presence.MinorFaction)
                         .FirstOrDefault(
                            dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                     && dgssmfg.Presence.MinorFaction == minorFaction
                                     && dgssmfg.Presence.StarSystem == starSystem);
            if (discordGuildStarSystemMinorFactionGoal == null)
            {
                Presence starSystemMinorFaction =
                    dbContext.Presences.First(
                        ssmf => ssmf.MinorFaction == minorFaction
                              && ssmf.StarSystem == starSystem);
                if (starSystemMinorFaction == null)
                {
                    starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction };
                    dbContext.Presences.Add(starSystemMinorFaction);
                }

                discordGuildStarSystemMinorFactionGoal = new()
                {
                    DiscordGuild = discordGuild,
                    Presence = starSystemMinorFaction
                };
                dbContext.DiscordGuildPresenceGoals.Add(discordGuildStarSystemMinorFactionGoal);
            }
            discordGuildStarSystemMinorFactionGoal.Goal = goalName;
            dbContext.SaveChanges();
            transactionScope.Complete();
        }
    }
}
