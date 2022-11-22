using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.Test.ToDo;
using OrderBot.ToDo;
using System.Reflection;
using System.Text.Json;
using System.Transactions;

namespace OrderBot.Test.CarrierMovement;

internal class CarrierMovementMessageProcessorTests
{
    [Test]
    public void Ctor()
    {
        using OrderBotDbContextFactory contextFactory = new();
        using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
        ILogger<CarrierMovementMessageProcessor> logger = NullLogger<CarrierMovementMessageProcessor>.Instance;
        IDiscordClient discordClient = Mock.Of<IDiscordClient>();
        TextChannelWriterFactory factory = new(discordClient);
        using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

        CarrierMovementMessageProcessor messageProcessor = new(dbContext,
            logger, factory, memoryCache);

        Assert.That(messageProcessor.Logger, Is.EqualTo(logger));
        Assert.That(messageProcessor.TextChannelWriterFactory, Is.EqualTo(factory));
        Assert.That(messageProcessor.DbContext, Is.EqualTo(dbContext));
    }

    [Test]
    public void CacheDuration()
    {
        Assert.That(CarrierMovementMessageProcessor.CacheDuration,
            Is.EqualTo(TimeSpan.FromMinutes(5)));
    }

    /// <summary>
    /// Test delegate used for <see cref="ProcessAsync"/>.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to populate with test data.
    /// </param>
    /// <returns>
    /// Test data used for checking results.
    /// </returns>
    public delegate (string resourceName, DiscordGuild discordGuild, StarSystem starSystem,
            IEnumerable<Carrier> expectedCarriers, IEnumerable<Carrier> expectedNewlyArrivedCarriers)
        PopulateProcessAsyncTestData(OrderBotDbContext dbContext);

    [Test]
    [TestCaseSource(nameof(ProcessAsync_Source))]
    public void ProcessAsync(PopulateProcessAsyncTestData populateTestData)
    {
        using OrderBotDbContextFactory contextFactory = new();
        using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

        (
            string resourceName,
            DiscordGuild discordGuild,
            StarSystem starSystem,
            IEnumerable<Carrier> expectedCarriers,
            IEnumerable<Carrier> expectedNewlyArrivedCarriers
        )
            = populateTestData(dbContext);

        FakeLogger<CarrierMovementMessageProcessor> logger = new();
        FakeTextChannelWriterFactory factory = new();

        using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        CarrierMovementMessageProcessor messageProcessor = new(dbContext, logger, factory, memoryCache);
        using Stream? stream = Assembly.GetExecutingAssembly()?.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            messageProcessor.ProcessAsync(JsonDocument.Parse(stream)).GetAwaiter().GetResult();
        }
        else
        {
            Assert.Fail("Test resource {resourceName} failed to load");
        }

        Assert.That(
                dbContext.Carriers,
                Is.EquivalentTo(expectedCarriers).Using(CarrierEqualityComparer.Instance));
        if (expectedNewlyArrivedCarriers.Any() && discordGuild.CarrierMovementChannel != null)
        {
            Assert.That(
                factory.ChannelToStringBuilder[discordGuild.CarrierMovementChannel ?? 0].ToString(),
                Is.EqualTo(
                    CarrierMovementMessageProcessor.GetCarrierMovementMessage(
                        starSystem, expectedNewlyArrivedCarriers)));
        }
        if (expectedCarriers.Any())
        {
            Assert.That(logger.LogEntries, Is.EquivalentTo(new LogEntry[]
            {
                new LogEntry(LogLevel.Information, new EventId(),
                    $"Carrier(s) {string.Join(", ", expectedCarriers.Where(c => c.StarSystem == starSystem).Select(c => c.Name).OrderBy(n => n))} in {starSystem.Name} updated",
                    null)
            }).Using(new LogEntryEqualityComparer()));
        }
        else
        {
            Assert.That(logger.LogEntries, Is.Empty);
        }
    }
    public static IEnumerable<TestCaseData> ProcessAsync_Source()
    {
        return new PopulateProcessAsyncTestData[]
        {
            NoPresenceOrGoal,
            Presence,
            Goal,
            AllIgnored,
            NoChannel,
            TwoSystems
        }.Select(f => new TestCaseData(f).SetName($"{nameof(ProcessAsync)} {f.Method.Name}"));
    }

    public static readonly string Ltt2684Message = "OrderBot.Test.samples.LTT 2684 FSS.json";
    public static readonly DateTime Ltt2684MessageTimeStamp = DateTime.Parse("2022-10-30T13:56:57Z").ToUniversalTime();

    private static (string, DiscordGuild, StarSystem, IEnumerable<Carrier>, IEnumerable<Carrier>) NoPresenceOrGoal(OrderBotDbContext dbContext)
    {
        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild", CarrierMovementChannel = 1234567890 };
        testGuild.SupportedMinorFactions.Add(minorFaction);
        StarSystem starSystem = new() { Name = "LTT 2684" };
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(starSystem);

        return (Ltt2684Message, testGuild, starSystem, Array.Empty<Carrier>(), Array.Empty<Carrier>());
    }

    private static (string, DiscordGuild, StarSystem, IEnumerable<Carrier>, IEnumerable<Carrier>) Presence(OrderBotDbContext dbContext)
    {
        Carrier cowboyB = new() { Name = "Cowboy B X9Z-B0B", FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) };
        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild", CarrierMovementChannel = 1234567890 };
        testGuild.IgnoredCarriers.Add(cowboyB);
        testGuild.SupportedMinorFactions.Add(minorFaction);
        StarSystem ltt2684 = new() { Name = "LTT 2684" };
        dbContext.Carriers.Add(cowboyB);
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(ltt2684);

        Presence presence = new() { MinorFaction = minorFaction, StarSystem = ltt2684, Influence = 0.1 };
        dbContext.Presences.Add(presence);
        dbContext.SaveChanges();

        Carrier[] expectedCarriers = new Carrier[]
        {
            new() { Name = "Cowboy B X9Z-B0B", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp }
        };

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers,
            expectedCarriers.Where(c => c.SerialNumber != cowboyB.SerialNumber));
    }

    private static (string, DiscordGuild, StarSystem, IEnumerable<Carrier>, IEnumerable<Carrier>) Goal(OrderBotDbContext dbContext)
    {
        Carrier cowboyB = new() { Name = "Cowboy B X9Z-B0B", FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) };
        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild", CarrierMovementChannel = 1234567890 };
        testGuild.IgnoredCarriers.Add(cowboyB);
        testGuild.SupportedMinorFactions.Add(minorFaction);
        StarSystem ltt2684 = new() { Name = "LTT 2684" };
        dbContext.Carriers.Add(cowboyB);
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(ltt2684);

        MinorFaction otherMinorFaction = new() { Name = "Other Minor Faction" };
        DiscordGuildPresenceGoal discordGuildPresenceGoal = new()
        {
            DiscordGuild = testGuild,
            Presence = new() { MinorFaction = otherMinorFaction, StarSystem = ltt2684, Influence = 0.1 },
            Goal = MaintainGoal.Instance.Name
        };
        dbContext.DiscordGuildPresenceGoals.Add(discordGuildPresenceGoal);
        dbContext.SaveChanges();

        Carrier[] expectedCarriers = new Carrier[]
        {
            new() { Name = "Cowboy B X9Z-B0B", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp }
        };

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers,
            expectedCarriers.Where(c => c.SerialNumber != cowboyB.SerialNumber));
    }

    private static (string, DiscordGuild, StarSystem, IEnumerable<Carrier>, IEnumerable<Carrier>) AllIgnored(OrderBotDbContext dbContext)
    {
        StarSystem ltt2684 = new() { Name = "LTT 2684" };

        Carrier[] existingCarriers = new Carrier[]
        {
            new() { Name = "Cowboy B X9Z-B0B", FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp }
        };

        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild", CarrierMovementChannel = 1234567890 };
        foreach (Carrier carrier in existingCarriers)
        {
            testGuild.IgnoredCarriers.Add(carrier);
        }
        testGuild.SupportedMinorFactions.Add(minorFaction);
        dbContext.Carriers.AddRange(existingCarriers);
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(ltt2684);
        Presence presence = new() { MinorFaction = minorFaction, StarSystem = ltt2684, Influence = 0.1 };
        dbContext.Presences.Add(presence);
        dbContext.SaveChanges();

        Carrier[] expectedCarriers = new Carrier[]
        {
            new() { Name = "Cowboy B X9Z-B0B", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp }
        };

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers, Array.Empty<Carrier>());
    }

    private static (string, DiscordGuild, StarSystem, IEnumerable<Carrier>, IEnumerable<Carrier>) NoChannel(OrderBotDbContext dbContext)
    {
        StarSystem ltt2684 = new() { Name = "LTT 2684" };

        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild" };
        testGuild.SupportedMinorFactions.Add(minorFaction);
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(ltt2684);

        Presence presence = new() { MinorFaction = minorFaction, StarSystem = ltt2684, Influence = 0.1 };
        dbContext.Presences.Add(presence);
        dbContext.SaveChanges();

        Carrier[] expectedCarriers = new Carrier[]
        {
            new() { Name = "Cowboy B X9Z-B0B", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) },
            new() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp  },
            new() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp  },
            new() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1)  }
        };

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers, Array.Empty<Carrier>());
    }

    private static (string, DiscordGuild, StarSystem, IEnumerable<Carrier>, IEnumerable<Carrier>) TwoSystems(OrderBotDbContext dbContext)
    {
        StarSystem ltt2684 = new() { Name = "LTT 2684" };
        StarSystem hr1597 = new() { Name = "HR 1597" };
        Carrier[] existingCarriers = new Carrier[]
        {
            new() { Name = "Cowboy B X9Z-B0B", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) },
            new() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = hr1597, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) },
            new() { Name = "ODIN W6B-94Z", StarSystem = hr1597, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) },
            new() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) },
            new() { Name = "PIZZA HUT 6HY-U7U", StarSystem = hr1597, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-2) }
        };

        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild" };
        testGuild.SupportedMinorFactions.Add(minorFaction);
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(ltt2684);
        dbContext.StarSystems.Add(hr1597);
        dbContext.Carriers.AddRange(existingCarriers);

        Presence presence = new() { MinorFaction = minorFaction, StarSystem = ltt2684, Influence = 0.1 };
        dbContext.Presences.Add(presence);
        dbContext.SaveChanges();

        Carrier[] expectedCarriers = new Carrier[]
        {
            new() { Name = "Cowboy B X9Z-B0B", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) },
            new() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) },
            new() { Name = "PIZZA HUT 6HY-U7U", StarSystem = hr1597, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-2) }
        };

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers, Array.Empty<Carrier>());
    }

    /// <summary>
    /// Test delegate used for <see cref="ProcessAsync"/>.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to populate with test data.
    /// </param>
    /// <returns>
    /// Test data used for checking results.
    /// </returns>
    public delegate IDictionary<string, IDictionary<int, ulong?>>
        PopulateMappingTestData(OrderBotDbContext dbContext);

    [Test]
    [TestCaseSource(nameof(ProcessMapping_Source))]
    public void GetSystemToGuildToChannel(PopulateMappingTestData populateTestData)
    {
        using OrderBotDbContextFactory contextFactory = new();
        using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

        IDictionary<string, IDictionary<int, ulong?>> expectedResult = populateTestData(dbContext);

        IDictionary<string, IDictionary<int, ulong?>> actualResult =
            CarrierMovementMessageProcessor.GetSystemToGuildToChannel(dbContext);

        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    public static IEnumerable<TestCaseData> ProcessMapping_Source()
    {
        return new PopulateMappingTestData[]
        {
            Empty,
            PresenceInOneSystem,
            PresenceAndGoal,
            Overlap
        }.Select(f => new TestCaseData(f).SetName($"{nameof(GetSystemToGuildToChannel)} {f.Method.Name}"));
    }

    public static IDictionary<string, IDictionary<int, ulong?>> Empty(OrderBotDbContext dbContext)
    {
        return new Dictionary<string, IDictionary<int, ulong?>>();
    }

    public static IDictionary<string, IDictionary<int, ulong?>> PresenceInOneSystem(OrderBotDbContext dbContext)
    {
        StarSystem sol = new() { Name = "Sol" };
        MinorFaction darkWheel = new() { Name = "Dark Wheel" };
        Presence presence = new() { MinorFaction = darkWheel, StarSystem = sol, Influence = 0.1 };
        dbContext.Presences.Add(presence);
        Carrier carrier = new() { Name = "HEART OF GOLD 6HY-OQ4" };
        DiscordGuild testGuid = new() { Name = "Test", CarrierMovementChannel = 9875264291 };
        testGuid.SupportedMinorFactions.Add(darkWheel);
        testGuid.IgnoredCarriers.Add(carrier);
        dbContext.DiscordGuilds.Add(testGuid);
        dbContext.SaveChanges();

        return new Dictionary<string, IDictionary<int, ulong?>>()
        {
            {
                sol.Name,
                new Dictionary<int, ulong?>()
                {
                    {
                        testGuid .Id,
                        testGuid.CarrierMovementChannel
                    }
                }
            }
        };
    }

    public static IDictionary<string, IDictionary<int, ulong?>> PresenceAndGoal(OrderBotDbContext dbContext)
    {
        Presence darkWheelInSol = new()
        {
            MinorFaction = new() { Name = "Dark Wheel" },
            StarSystem = new() { Name = "Sol" },
            Influence = 0.1
        };
        dbContext.Presences.Add(darkWheelInSol);
        DiscordGuild firstGuild = new() { Name = "First", GuildId = 20982408923432, CarrierMovementChannel = 9875264291 };
        firstGuild.SupportedMinorFactions.Add(darkWheelInSol.MinorFaction);
        dbContext.DiscordGuilds.Add(firstGuild);

        DiscordGuildPresenceGoal maintainDarkWheelInAlphaCentauri = new()
        {
            DiscordGuild = new() { Name = "Second", GuildId = 982340923874 },
            Presence = new()
            {
                MinorFaction = new() { Name = "Hutton Truckers" },
                StarSystem = new() { Name = "Alpha Centauri" },
                Influence = 0.2
            },
            Goal = MaintainGoal.Instance.Name
        };
        dbContext.DiscordGuildPresenceGoals.Add(maintainDarkWheelInAlphaCentauri);

        dbContext.SaveChanges();

        return new Dictionary<string, IDictionary<int, ulong?>>()
        {
            {
                darkWheelInSol.StarSystem.Name,
                new Dictionary<int, ulong?>()
                {
                    {
                        firstGuild.Id,
                        firstGuild.CarrierMovementChannel
                    }
                }
            },
            {
                maintainDarkWheelInAlphaCentauri.Presence.StarSystem.Name,
                new Dictionary<int, ulong?>()
                {
                    {
                        maintainDarkWheelInAlphaCentauri.DiscordGuild.Id,
                        maintainDarkWheelInAlphaCentauri.DiscordGuild.CarrierMovementChannel
                    }
                }
            }
        };
    }

    public static IDictionary<string, IDictionary<int, ulong?>> Overlap(OrderBotDbContext dbContext)
    {
        Presence darkWheelInSol = new()
        {
            MinorFaction = new() { Name = "Dark Wheel" },
            StarSystem = new() { Name = "Sol" },
            Influence = 0.1
        };
        dbContext.Presences.Add(darkWheelInSol);
        DiscordGuild firstGuild = new() { Name = "First", GuildId = 20982408923432, CarrierMovementChannel = 9875264291 };
        firstGuild.SupportedMinorFactions.Add(darkWheelInSol.MinorFaction);
        dbContext.DiscordGuilds.Add(firstGuild);

        DiscordGuildPresenceGoal maintainDarkWheelInAlphaCentauri = new()
        {
            DiscordGuild = new() { Name = "Second", GuildId = 982340923874 },
            Presence = new()
            {
                MinorFaction = new() { Name = "Hutton Truckers" },
                StarSystem = darkWheelInSol.StarSystem,
                Influence = 0.2
            },
            Goal = MaintainGoal.Instance.Name
        };
        dbContext.DiscordGuildPresenceGoals.Add(maintainDarkWheelInAlphaCentauri);

        dbContext.SaveChanges();

        return new Dictionary<string, IDictionary<int, ulong?>>()
        {
            {
                darkWheelInSol.StarSystem.Name,
                new Dictionary<int, ulong?>()
                {
                    {
                        firstGuild.Id,
                        firstGuild.CarrierMovementChannel
                    },
                    {
                        maintainDarkWheelInAlphaCentauri.DiscordGuild.Id,
                        maintainDarkWheelInAlphaCentauri.DiscordGuild.CarrierMovementChannel
                    }
                }
            }
        };
    }
}
