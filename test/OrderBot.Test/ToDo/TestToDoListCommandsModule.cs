using Discord;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;
using System.Transactions;
using Goal = OrderBot.ToDo.Goal;

namespace OrderBot.Test.ToDo
{
    internal class TestToDoListCommandsModule
    {
        [Test]
        public void Add_Empty()
        {
            using OrderBotDbContextFactory contextFactory = new();
            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            StarSystem starSystem = new() { Name = "Alpha Centauri" };
            dbContext.StarSystems.Add(starSystem);
            dbContext.SaveChanges();

            MinorFaction minorFaction = new() { Name = "Hutton Truckers" };
            dbContext.MinorFactions.Add(minorFaction);
            dbContext.SaveChanges();

            Goal goal = Goals.Default;

            const ulong testGuildId = 1234567890;
            const string testGuildName = "My Discord Server";
            IGuild guild = Mock.Of<IGuild>(g => g.Id == testGuildId && g.Name == testGuildName);

            string minorFactionName = minorFaction.Name;
            string starSystemName = starSystem.Name;
            string goalName = goal.Name;
            ToDoListCommandsModule.Goals.AddImplementation(dbContext, guild, minorFactionName, starSystemName, goalName);

            DiscordGuildStarSystemMinorFactionGoal? discordGuildStarSystemMinorFactionGoal =
                dbContext.DiscordGuildStarSystemMinorFactionGoals.Include(dgssmfg => dgssmfg.StarSystemMinorFaction)
                                                                 .Include(dgssmfg => dgssmfg.DiscordGuild)
                                                                 .FirstOrDefault(dgssmfg => dgssmfg.DiscordGuild.GuildId == testGuildId
                                                                                         && dgssmfg.StarSystemMinorFaction.StarSystem.Name == starSystemName
                                                                                         && dgssmfg.StarSystemMinorFaction.MinorFaction.Name == minorFactionName);
            if (discordGuildStarSystemMinorFactionGoal != null)
            {
                Assert.That(discordGuildStarSystemMinorFactionGoal.Goal == goal.Name);
                Assert.That(discordGuildStarSystemMinorFactionGoal.DiscordGuild.Name == guild.Name);
                Assert.That(discordGuildStarSystemMinorFactionGoal.DiscordGuild.GuildId == guild.Id);
                Assert.That(discordGuildStarSystemMinorFactionGoal.StarSystemMinorFaction, Is.Not.Null);
                Assert.That(discordGuildStarSystemMinorFactionGoal.StarSystemMinorFaction.StarSystem, Is.EqualTo(starSystem));
                Assert.That(discordGuildStarSystemMinorFactionGoal.StarSystemMinorFaction.MinorFaction, Is.EqualTo(minorFaction));
                Assert.That(discordGuildStarSystemMinorFactionGoal.StarSystemMinorFaction.States, Is.Empty);
            }
            else
            {
                Assert.Fail($"{nameof(discordGuildStarSystemMinorFactionGoal)} is null");
            }
        }

        [Test]
        public void Add_Existing()
        {
            using OrderBotDbContextFactory contextFactory = new();
            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            const ulong testGuildId = 1234567890;
            const string testGuildName = "My Discord Server";
            DiscordGuild discordGuild = new() { Name = testGuildName, GuildId = testGuildId };

            StarSystem starSystem = new() { Name = "Alpha Centauri" };
            dbContext.StarSystems.Add(starSystem);
            dbContext.SaveChanges();

            MinorFaction minorFaction = new() { Name = "Hutton Truckers" };
            dbContext.MinorFactions.Add(minorFaction);
            dbContext.SaveChanges();

            Goal goal = Goals.Default;

            IGuild guild = Mock.Of<IGuild>(g => g.Id == testGuildId && g.Name == testGuildName);

            StarSystemMinorFaction starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction };
            dbContext.StarSystemMinorFactions.Add(starSystemMinorFaction);
            dbContext.SaveChanges();

            DiscordGuildStarSystemMinorFactionGoal discordGuildStarSystemMinorFactionGoal = new()
            {
                DiscordGuild = discordGuild,
                StarSystemMinorFaction = starSystemMinorFaction,
                Goal = goal.Name
            };
            dbContext.DiscordGuildStarSystemMinorFactionGoals.Add(discordGuildStarSystemMinorFactionGoal);
            dbContext.SaveChanges();

            ToDoListCommandsModule.Goals.AddImplementation(dbContext, guild, minorFaction.Name, starSystem.Name, goal.Name);

            DiscordGuildStarSystemMinorFactionGoal? newDiscordGuildStarSystemMinorFactionGoal =
                dbContext.DiscordGuildStarSystemMinorFactionGoals.Include(dgssmfg => dgssmfg.StarSystemMinorFaction)
                                                                 .Include(dgssmfg => dgssmfg.DiscordGuild)
                                                                 .FirstOrDefault(dgssmfg => dgssmfg.DiscordGuild.GuildId == testGuildId
                                                                                         && dgssmfg.StarSystemMinorFaction.StarSystem.Name == starSystem.Name
                                                                                         && dgssmfg.StarSystemMinorFaction.MinorFaction.Name == minorFaction.Name);
            if (newDiscordGuildStarSystemMinorFactionGoal != null)
            {
                Assert.That(newDiscordGuildStarSystemMinorFactionGoal.Goal == goal.Name);
                Assert.That(newDiscordGuildStarSystemMinorFactionGoal.DiscordGuild, Is.EqualTo(discordGuild));
                Assert.That(newDiscordGuildStarSystemMinorFactionGoal.StarSystemMinorFaction, Is.EqualTo(starSystemMinorFaction));
            }
            else
            {
                Assert.Fail($"{nameof(newDiscordGuildStarSystemMinorFactionGoal)} is null");
            }
        }
    }
}
