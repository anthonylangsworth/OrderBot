using Discord;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.EntityFramework;
using OrderBot.ToDo;
using System.Transactions;
using Goal = OrderBot.ToDo.Goal;

namespace OrderBot.Test.ToDo;

internal class ToDoListApiTests
{
    [Test]
    public void Add_Empty()
    {
        using OrderBotDbContextFactory contextFactory = new();
        using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
        using TransactionScope transactionScope = new();
        ToDoListApi api = new(
            new ToDoListGenerator(contextFactory),
            new ToDoListFormatter());

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
        api.AddGoals(dbContext, guild,
            new[] { (minorFactionName, starSystemName, goalName) });

        DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
            dbContext.DiscordGuildPresenceGoals.Include(dgssmfg => dgssmfg.Presence)
                                                             .Include(dgssmfg => dgssmfg.DiscordGuild)
                                                             .FirstOrDefault(dgssmfg => dgssmfg.DiscordGuild.GuildId == testGuildId
                                                                                     && dgssmfg.Presence.StarSystem.Name == starSystemName
                                                                                     && dgssmfg.Presence.MinorFaction.Name == minorFactionName);
        if (discordGuildStarSystemMinorFactionGoal != null)
        {
            Assert.That(discordGuildStarSystemMinorFactionGoal.Goal == goal.Name);
            Assert.That(discordGuildStarSystemMinorFactionGoal.DiscordGuild.Name == guild.Name);
            Assert.That(discordGuildStarSystemMinorFactionGoal.DiscordGuild.GuildId == guild.Id);
            Assert.That(discordGuildStarSystemMinorFactionGoal.Presence, Is.Not.Null);
            Assert.That(discordGuildStarSystemMinorFactionGoal.Presence.StarSystem, Is.EqualTo(starSystem));
            Assert.That(discordGuildStarSystemMinorFactionGoal.Presence.MinorFaction, Is.EqualTo(minorFaction));
            Assert.That(discordGuildStarSystemMinorFactionGoal.Presence.States, Is.Empty);
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
        ToDoListApi api = new(
            new ToDoListGenerator(contextFactory),
            new ToDoListFormatter());
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

        Presence starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction };
        dbContext.Presences.Add(starSystemMinorFaction);
        dbContext.SaveChanges();

        DiscordGuildPresenceGoal discordGuildStarSystemMinorFactionGoal = new()
        {
            DiscordGuild = discordGuild,
            Presence = starSystemMinorFaction,
            Goal = goal.Name
        };
        dbContext.DiscordGuildPresenceGoals.Add(discordGuildStarSystemMinorFactionGoal);
        dbContext.SaveChanges();
        api.AddGoals(dbContext, guild,
            new[] { (minorFaction.Name, starSystem.Name, goal.Name) });

        DiscordGuildPresenceGoal? newDiscordGuildStarSystemMinorFactionGoal =
            dbContext.DiscordGuildPresenceGoals.Include(dgssmfg => dgssmfg.Presence)
                                                             .Include(dgssmfg => dgssmfg.DiscordGuild)
                                                             .FirstOrDefault(dgssmfg => dgssmfg.DiscordGuild.GuildId == testGuildId
                                                                                     && dgssmfg.Presence.StarSystem.Name == starSystem.Name
                                                                                     && dgssmfg.Presence.MinorFaction.Name == minorFaction.Name);
        if (newDiscordGuildStarSystemMinorFactionGoal != null)
        {
            Assert.That(newDiscordGuildStarSystemMinorFactionGoal.Goal == goal.Name);
            Assert.That(newDiscordGuildStarSystemMinorFactionGoal.DiscordGuild, Is.EqualTo(discordGuild));
            Assert.That(newDiscordGuildStarSystemMinorFactionGoal.Presence, Is.EqualTo(starSystemMinorFaction));
        }
        else
        {
            Assert.Fail($"{nameof(newDiscordGuildStarSystemMinorFactionGoal)} is null");
        }
    }
}
