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
    /// Test delegate used for <see cref="Process"/>.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to populate with test data.
    /// </param>
    /// <returns>
    /// Test data used for checking results.
    /// </returns>
    public delegate (string resourceName, DiscordGuild discordGuild, StarSystem starSystem,
            IEnumerable<Carrier> expectedCarriers, IEnumerable<Carrier> expectedNewlyArrivedCarriers)
        PopulateTestData(OrderBotDbContext dbContext);

    [Test]
    [TestCaseSource(nameof(Process_Source))]
    public void Process(PopulateTestData populateTestData)
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

        FakeLogger<CarrierMovementMessageProcessor> fakeLoger = new();
        FakeTextChannelWriterFactory factory = new();

        using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        CarrierMovementMessageProcessor messageProcessor = new(dbContext, fakeLoger, factory, memoryCache);
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
        Assert.That(factory.StringBuilder.ToString(),
            Is.EqualTo(CarrierMovementMessageProcessor.GetCarrierMovementMessage(
                starSystem, expectedNewlyArrivedCarriers)));
        if (expectedCarriers.Any())
        {
            Assert.That(fakeLoger.LogEntries, Is.EquivalentTo(new LogEntry[]
            {
                new LogEntry(LogLevel.Information, new EventId(),
                    $"Carrier(s) {string.Join(", ", expectedCarriers.Select(c => c.Name).OrderBy(n => n))} in {starSystem.Name} updated",
                    null)
            }).Using(new LogEntryEqualityComparer()));
        }
        else
        {
            Assert.That(fakeLoger.LogEntries, Is.Empty);
        }
    }

    public static IEnumerable<TestCaseData> Process_Source()
    {
        return new PopulateTestData[]
        {
            NoPresence,
            Presence,
            Goal,
            AllIgnored,
            NoChannel
        }.Select(f => new TestCaseData(f).SetName(f.Method.Name));
    }

    public static readonly string Ltt2684Message = "OrderBot.Test.samples.LTT 2684 FSS.json";
    public static readonly DateTime Ltt2684MessageTimeStamp = DateTime.Parse("2022-10-30T13:56:57Z").ToUniversalTime();

    private static (string, DiscordGuild, StarSystem, IEnumerable<Carrier>, IEnumerable<Carrier>) NoPresence(OrderBotDbContext dbContext)
    {
        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild", CarrierMovementChannel = 1234567890 };
        testGuild.SupportedMinorFactions.Add(minorFaction);
        StarSystem starSystem = new() { Name = "LTT 2684" };
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(starSystem);

        Carrier[] expectedCarriers = Array.Empty<Carrier>();

        return (Ltt2684Message, testGuild, starSystem, expectedCarriers, Array.Empty<Carrier>());
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
            cowboyB, // Ignored, so should not update
            new Carrier() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new Carrier() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new Carrier() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp }
        };

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers,
            expectedCarriers.Where(c => c.FirstSeen == Ltt2684MessageTimeStamp));
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
            cowboyB, // Ignored, so should not update
            new Carrier() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new Carrier() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new Carrier() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp }
        };

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers,
            expectedCarriers.Where(c => c.FirstSeen == Ltt2684MessageTimeStamp));
    }

    private static (string, DiscordGuild, StarSystem, IEnumerable<Carrier>, IEnumerable<Carrier>) AllIgnored(OrderBotDbContext dbContext)
    {
        StarSystem ltt2684 = new() { Name = "LTT 2684" };

        Carrier[] expectedCarriers = new Carrier[]
        {
            new Carrier() { Name = "Cowboy B X9Z-B0B", FirstSeen = Ltt2684MessageTimeStamp },
            new Carrier() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new Carrier() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new Carrier() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp }
        };

        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild", CarrierMovementChannel = 1234567890 };
        foreach (Carrier carrier in expectedCarriers)
        {
            testGuild.IgnoredCarriers.Add(carrier);
        }
        testGuild.SupportedMinorFactions.Add(minorFaction);
        dbContext.Carriers.AddRange(expectedCarriers);
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(ltt2684);
        Presence presence = new() { MinorFaction = minorFaction, StarSystem = ltt2684, Influence = 0.1 };
        dbContext.Presences.Add(presence);
        dbContext.SaveChanges();

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers, Array.Empty<Carrier>());
    }

    private static (string, DiscordGuild, StarSystem, IEnumerable<Carrier>, IEnumerable<Carrier>) NoChannel(OrderBotDbContext dbContext)
    {
        StarSystem ltt2684 = new() { Name = "LTT 2684" };
        Carrier[] expectedCarriers = new Carrier[]
        {
            new Carrier() { Name = "Cowboy B X9Z-B0B", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp.AddDays(-1) },
            new Carrier() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new Carrier() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp },
            new Carrier() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = Ltt2684MessageTimeStamp }
        };

        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild" };
        testGuild.SupportedMinorFactions.Add(minorFaction);
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(ltt2684);

        Presence presence = new() { MinorFaction = minorFaction, StarSystem = ltt2684, Influence = 0.1 };
        dbContext.Presences.Add(presence);
        dbContext.SaveChanges();

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers, Array.Empty<Carrier>());
    }
}
