﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Test.MessageProcessors;
using OrderBot.ToDo;
using System.Reflection;
using System.Text.Json;

namespace OrderBot.Test.ToDo;

internal class ToDoListMessageProcessorTests : DbTest
{
    [Test]
    public void Ctor()
    {
        ILogger<ToDoListMessageProcessor> logger = NullLogger<ToDoListMessageProcessor>.Instance;
        SupportedMinorFactionsCache supportedMinorFactionsCache = new(MemoryCache);
        GoalStarSystemsCache goalSystemsCache = new(MemoryCache);

        ToDoListMessageProcessor systemMinorFactionMessageProcessor = new(DbContext, logger,
            supportedMinorFactionsCache, goalSystemsCache);
        Assert.That(systemMinorFactionMessageProcessor.Logger, Is.EqualTo(logger));
        Assert.That(systemMinorFactionMessageProcessor.DbContext, Is.EqualTo(DbContext));
    }

    // TODO: Test ProcessAsync

    [Test]
    public void GetBgsData_MatchingMinorFaction()
    {
        SupportedMinorFactionsCache supportedMinorFactionsCache = new(MemoryCache);
        GoalStarSystemsCache goalSystemsCache = new(MemoryCache);

        DiscordGuild discordGuild = new();
        discordGuild.SupportedMinorFactions.Add(new() { Name = "Ross 199 Silver Raiders" });
        DbContext.DiscordGuilds.Add(discordGuild);
        DbContext.SaveChanges();

        using Stream? stream = Assembly.GetExecutingAssembly()?.GetManifestResourceStream("OrderBot.Test.Samples.Ross 199.json");
        if (stream != null)
        {
            EddnStarSystemData? bgsStarSystemData = ToDoListMessageProcessor.GetBgsData(DbContext, JsonDocument.Parse(stream),
                supportedMinorFactionsCache, goalSystemsCache);

            if (bgsStarSystemData != null)
            {
                Assert.That(bgsStarSystemData.StarSystemName, Is.EqualTo("Ross 199"));
                Assert.That(
                    DbDateTimeComparer.Instance.Equals(
                        DateTime.Parse("2022-10-25T12:22:42.685555Z").ToUniversalTime(),
                        bgsStarSystemData.Timestamp), Is.True);
                Assert.That(bgsStarSystemData.MinorFactionDetails, Is.EquivalentTo(
                    new EddnMinorFactionInfluence[]
                    {
                        new ()
                        {
                            MinorFaction = "Ross 199 Silver Major Limited",
                            Influence = 0.055833,
                            States = Array.Empty<string>()
                        },
                        new ()
                        {
                            MinorFaction = "Ross 199 Silver Raiders",
                            Influence = 0.00997,
                            States = new string[] { "Bust" }
                        },
                        new ()
                        {
                            MinorFaction = "Allied Midgard Nationalists",
                            Influence = 0.062812,
                            States = Array.Empty<string>()
                        },
                        new ()
                        {
                            MinorFaction = "Ross 199 Law Party",
                            Influence = 0.065803,
                            States = Array.Empty<string>()
                        },
                        new ()
                        {
                            MinorFaction = "Future of Ross 199",
                            Influence = 0.127617,
                            States = Array.Empty<string>()
                        },
                        new ()
                        {
                            MinorFaction = "Merry Band of Awesome",
                            Influence = 0.586241,
                            States = Array.Empty<string>()
                        },
                        new ()
                        {
                            MinorFaction = "Predator Mining Syndicate",
                            Influence = 0.091725,
                            States = Array.Empty<string>()
                        }
                    }).Using(EddnMinorFactionInfluenceEqualityComparer.Instance));
                Assert.That(bgsStarSystemData.SystemSecurityLevel, Is.EqualTo("$SYSTEM_SECURITY_medium;"));
                Assert.That(bgsStarSystemData.Conflicts, Is.EquivalentTo(
                    new EddnConflict[]
                    {
                        new EddnConflict()
                        {
                            Faction1 = new EddnConflictFaction()
                            {
                                Name = "Ross 199 Silver Major Limited",
                                Stake = "Kroehl Orbital",
                                WonDays = 0
                            },
                            Faction2 = new EddnConflictFaction()
                            {
                                Name = "Future of Ross 199",
                                Stake = "Heaviside Bastion",
                                WonDays = 1
                            },
                            Status = "",
                            WarType = "civilwar"
                        }
                    }).Using(EddnConflictEqualityComparer.Instance));
            }
            else
            {
                Assert.Fail("No BGS data");
            }
        }
        else
        {
            Assert.Fail("Cannot load resource");
        }
    }

    [Test]
    public void GetBgsData_NoMatchingMinorFactions()
    {
        SupportedMinorFactionsCache supportedMinorFactionsCache = new(MemoryCache);
        GoalStarSystemsCache goalSystemsCache = new(MemoryCache);

        // HashSet<string> supportedMinorFactions = new(new[] { "Foo" });
        using Stream? stream = Assembly.GetExecutingAssembly()?.GetManifestResourceStream("OrderBot.Test.Samples.Ross 199.json");
        if (stream != null)
        {
            EddnStarSystemData? bgsStarSystemData = ToDoListMessageProcessor.GetBgsData(DbContext,
                JsonDocument.Parse(stream), supportedMinorFactionsCache, goalSystemsCache);
            Assert.That(bgsStarSystemData, Is.Null);
        }
    }

    [Test]
    public void Update_NewSystem()
    {
        const string starSystem = "A";
        const string minorFaction = "B";
        const double newInfluence = 0.7;
        const string systemSecurity = "$SYSTEM_SECURITY";
        string[] states = new string[] { "C", "D" };
        DateTime timestamp = DateTime.UtcNow.ToUniversalTime();

        ILogger<ToDoListMessageProcessor> logger = new NullLogger<ToDoListMessageProcessor>();

        ToDoListMessageProcessor.Update(DbContext, new EddnStarSystemData()
        {
            Timestamp = timestamp,
            StarSystemName = starSystem,
            MinorFactionDetails = new[]
            {
                new EddnMinorFactionInfluence()
                {
                    MinorFaction = minorFaction,
                    Influence = newInfluence,
                    States = states
                }
            },
            SystemSecurityLevel = systemSecurity
        });
        IEnumerable<Presence> systemMinorFactions = DbContext.Presences.Include(smf => smf.States)
                                                                       .Include(smf => smf.StarSystem)
                                                                       .Include(smf => smf.MinorFaction)
                                                                       .Where(smf => smf.StarSystem.Name == starSystem);
        Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
        Presence? newSystemMinorFaction = systemMinorFactions.First();
        Assert.That(newSystemMinorFaction.StarSystem, Is.Not.Null);
        Assert.That(newSystemMinorFaction.StarSystem.Name, Is.EqualTo(starSystem));
        Assert.That(newSystemMinorFaction.StarSystem.LastUpdated, Is.EqualTo(timestamp).Within(DbDateTimeComparer.Epsilon));
        Assert.That(newSystemMinorFaction.MinorFaction, Is.Not.Null);
        Assert.That(newSystemMinorFaction.MinorFaction.Name, Is.EqualTo(minorFaction));
        Assert.That(newSystemMinorFaction.Influence, Is.EqualTo(newInfluence));
        Assert.That(newSystemMinorFaction.States.Select(state => state.Name), Is.EquivalentTo(states));
        Assert.That(newSystemMinorFaction.SecurityLevel, Is.EqualTo(systemSecurity));
    }

    [Test]
    public void Update_TwoSystems()
    {
        string starSystem1 = "A";
        string starSystem2 = "B";
        EddnMinorFactionInfluence systemOneMinorFactionInfo = new() { MinorFaction = "MF1", Influence = 0.3, States = ["A", "B"] };
        EddnMinorFactionInfluence systemTwoMinorFactionInfo = new() { MinorFaction = "MF2", Influence = 0.5, States = ["B"] };
        string[] states = new string[] { "C", "D" };
        DateTime timestamp = DateTime.UtcNow.ToUniversalTime();
        const string system1Security = "$SYSTEM_SECURITY_MEDIUM";
        const string system2Security = "$SYSTEM_SECURITY_LOW";

        ILogger<ToDoListMessageProcessor> logger = new NullLogger<ToDoListMessageProcessor>();

        ToDoListMessageProcessor.Update(DbContext, new EddnStarSystemData()
        {
            Timestamp = timestamp,
            StarSystemName = starSystem1,
            MinorFactionDetails = new[]
            {
                systemOneMinorFactionInfo
            },
            SystemSecurityLevel = system1Security
        });
        ToDoListMessageProcessor.Update(DbContext, new EddnStarSystemData()
        {
            Timestamp = timestamp,
            StarSystemName = starSystem2,
            MinorFactionDetails = new[]
            {
                systemTwoMinorFactionInfo
            },
            SystemSecurityLevel = system2Security
        });
        List<Presence> systemMinorFactions = DbContext.Presences.Include(smf => smf.States)
                                                                                            .Include(smf => smf.StarSystem)
                                                                                            .Include(smf => smf.MinorFaction)
                                                                                            .Where(smf => smf.StarSystem.Name == starSystem1 || smf.StarSystem.Name == starSystem2)
                                                                                            .ToList();
        Assert.That(systemMinorFactions.Count, Is.EqualTo(2));
        Assert.That(Helpers.IsSame(systemMinorFactions[0], starSystem1, timestamp, systemOneMinorFactionInfo), Is.True);
        Assert.That(Helpers.IsSame(systemMinorFactions[1], starSystem2, timestamp, systemTwoMinorFactionInfo), Is.True);
    }

    [Test]
    public void Update_TwoFactionsInOneSystem()
    {
        const string starSystem = "Alpha Centauri";
        const string minorFaction1 = "Alpha Aspirants";
        const string minorFaction2 = "Proxima People";
        const double minorFaction1Influence = 0.7;
        const double minorFaction2Influence = 0.3;
        string[] minorFaction1States = new string[] { "Boom" };
        string[] minorFaction2States = new string[] { "Bust", "Lockdown" };
        DateTime timestamp = DateTime.UtcNow.ToUniversalTime();
        string systemSecurityLevel = SecurityLevel.Low;

        ILogger<ToDoListMessageProcessor> logger = new NullLogger<ToDoListMessageProcessor>();

        ToDoListMessageProcessor.Update(DbContext, new EddnStarSystemData()
        {
            Timestamp = timestamp,
            StarSystemName = starSystem,
            MinorFactionDetails = new[]
            {
                new EddnMinorFactionInfluence()
                {
                    MinorFaction = minorFaction1,
                    Influence = minorFaction1Influence,
                    States = minorFaction1States
                },
                new EddnMinorFactionInfluence()
                {
                    MinorFaction = minorFaction2,
                    Influence = minorFaction2Influence,
                    States = minorFaction2States
                }
            },
            SystemSecurityLevel = systemSecurityLevel
        });
        IList<Presence> systemMinorFactions = DbContext.Presences.Include(smf => smf.States)
                                                                 .Include(smf => smf.StarSystem)
                                                                 .Include(smf => smf.MinorFaction)
                                                                 .Where(smf => smf.StarSystem.Name == starSystem)
                                                                 .OrderByDescending(smf => smf.Influence)
                                                                 .ToList();
        Assert.That(systemMinorFactions.Count, Is.EqualTo(2));

        Presence? newSystemMinorFaction1 = systemMinorFactions[0];
        Assert.That(newSystemMinorFaction1.StarSystem, Is.Not.Null);
        Assert.That(newSystemMinorFaction1.StarSystem.Name, Is.EqualTo(starSystem));
        Assert.That(newSystemMinorFaction1.StarSystem.LastUpdated, Is.EqualTo(timestamp).Within(DbDateTimeComparer.Epsilon));
        Assert.That(newSystemMinorFaction1.MinorFaction, Is.Not.Null);
        Assert.That(newSystemMinorFaction1.MinorFaction.Name, Is.EqualTo(minorFaction1));
        Assert.That(newSystemMinorFaction1.Influence, Is.EqualTo(minorFaction1Influence));
        Assert.That(newSystemMinorFaction1.States.Select(state => state.Name), Is.EquivalentTo(minorFaction1States));
        Assert.That(newSystemMinorFaction1.SecurityLevel, Is.EqualTo(systemSecurityLevel));

        Presence? newSystemMinorFaction2 = systemMinorFactions[1];
        Assert.That(newSystemMinorFaction2.StarSystem, Is.Not.Null);
        Assert.That(newSystemMinorFaction2.StarSystem.Name, Is.EqualTo(starSystem));
        Assert.That(newSystemMinorFaction2.StarSystem.LastUpdated, Is.EqualTo(timestamp).Within(DbDateTimeComparer.Epsilon));
        Assert.That(newSystemMinorFaction2.MinorFaction, Is.Not.Null);
        Assert.That(newSystemMinorFaction2.MinorFaction.Name, Is.EqualTo(minorFaction2));
        Assert.That(newSystemMinorFaction2.Influence, Is.EqualTo(minorFaction2Influence));
        Assert.That(newSystemMinorFaction2.States.Select(state => state.Name), Is.EquivalentTo(minorFaction2States));
        Assert.That(newSystemMinorFaction2.SecurityLevel, Is.Null);
    }
}
