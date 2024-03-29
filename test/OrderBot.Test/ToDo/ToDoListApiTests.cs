﻿using Discord;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;
using Goal = OrderBot.ToDo.Goal;

namespace OrderBot.Test.ToDo;

internal class ToDoListApiTests : DbTest
{
    [Test]
    public async Task SupportedFaction()
    {
        MinorFaction minorFaction = new() { Name = "Hutton Orbital Truckers Co-operative" };
        DbContext.MinorFactions.Add(minorFaction);
        DbContext.SaveChanges();

        const ulong testGuildId = 1234567890;
        const string testGuildName = "My Discord Server";
        IGuild guild = Mock.Of<IGuild>(g => g.Id == testGuildId && g.Name == testGuildName);

        ToDoListApi api = new(DbContext, guild, new FakeValidator());

        Assert.That(() => api.GetTodoList(), Throws.TypeOf<NoSupportedMinorFactionException>());
        Assert.That(api.GetSupportedMinorFaction(), Is.Null);
        await api.SetSupportedMinorFactionAsync(minorFaction.Name);
        Assert.That(() => api.GetTodoList(), Throws.Nothing);
        Assert.That(api.GetSupportedMinorFaction(), Is.EqualTo(minorFaction));
        api.ClearSupportedMinorFaction();
        Assert.That(() => api.GetTodoList(), Throws.TypeOf<NoSupportedMinorFactionException>());
        Assert.That(api.GetSupportedMinorFaction(), Is.Null);
    }

    [Test]
    public async Task AddGoals_Empty()
    {
        StarSystem starSystem = new() { Name = "Alpha Centauri" };
        DbContext.StarSystems.Add(starSystem);
        DbContext.SaveChanges();

        MinorFaction minorFaction = new() { Name = "Hutton Truckers" };
        DbContext.MinorFactions.Add(minorFaction);
        DbContext.SaveChanges();

        Goal goal = Goals.Default;

        const ulong testGuildId = 1234567890;
        const string testGuildName = "My Discord Server";
        IGuild guild = Mock.Of<IGuild>(g => g.Id == testGuildId && g.Name == testGuildName);

        ToDoListApi api = new(DbContext, guild, new FakeValidator());

        string minorFactionName = minorFaction.Name;
        string starSystemName = starSystem.Name;
        string goalName = goal.Name;
        await api.AddGoals(new[] { (minorFactionName, starSystemName, goalName) });

        DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
            DbContext.DiscordGuildPresenceGoals.Include(dgssmfg => dgssmfg.Presence)
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
    public async Task AddGoals_Existing()
    {
        const ulong testGuildId = 1234567890;
        const string testGuildName = "My Discord Server";
        DiscordGuild discordGuild = new() { Name = testGuildName, GuildId = testGuildId };

        StarSystem starSystem = new() { Name = "Alpha Centauri" };
        DbContext.StarSystems.Add(starSystem);
        DbContext.SaveChanges();

        MinorFaction minorFaction = new() { Name = "Hutton Truckers" };
        DbContext.MinorFactions.Add(minorFaction);
        DbContext.SaveChanges();

        Goal goal = Goals.Default;

        IGuild guild = Mock.Of<IGuild>(g => g.Id == testGuildId && g.Name == testGuildName);

        ToDoListApi api = new(DbContext, guild, new FakeValidator());

        Presence starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction };
        DbContext.Presences.Add(starSystemMinorFaction);
        DbContext.SaveChanges();

        DiscordGuildPresenceGoal discordGuildStarSystemMinorFactionGoal = new()
        {
            DiscordGuild = discordGuild,
            Presence = starSystemMinorFaction,
            Goal = goal.Name
        };
        DbContext.DiscordGuildPresenceGoals.Add(discordGuildStarSystemMinorFactionGoal);
        DbContext.SaveChanges();
        await api.AddGoals(new[] { (minorFaction.Name, starSystem.Name, goal.Name) });

        DiscordGuildPresenceGoal? newDiscordGuildStarSystemMinorFactionGoal =
            DbContext.DiscordGuildPresenceGoals.Include(dgssmfg => dgssmfg.Presence)
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
