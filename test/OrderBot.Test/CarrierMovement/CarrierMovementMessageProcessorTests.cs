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
    public delegate (string resourceName, DiscordGuild discordGuild, StarSystem starSystem, Carrier[] expectedCarriers, bool expectNotification)
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
            Carrier[] expectedCarriers,
            bool expectNotifications
        )
            = populateTestData(dbContext);

        MockRepository mockRepository = new(MockBehavior.Strict);
        Mock<ILogger<CarrierMovementMessageProcessor>> mockLogger =
            mockRepository.Create<ILogger<CarrierMovementMessageProcessor>>(MockBehavior.Loose);
        //mockLogger.Setup(l => l.Log(LogLevel.Information, 0,
        //    "Carrier(s) Cowboy B X9Z-B0B, T.N.V.A COSMOS HNV-L7X, E.D.A. WALKABOUT KHF-79Z, ODIN W6B-94Z in LTT 2684 updated"));
        ILogger<CarrierMovementMessageProcessor> logger = mockLogger.Object;

        Mock<TextChannelWriterFactory> mockFactory = mockRepository.Create<TextChannelWriterFactory>(null);
        if (expectNotifications)
        {
            Mock<TextChannelWriter> mockTextChannelWriter = mockRepository.Create<TextChannelWriter>(null);
            mockTextChannelWriter.Setup(
                tcw => tcw.WriteLine(
                    CarrierMovementMessageProcessor.GetCarrierMovementMessage(
                        starSystem,
                        expectedCarriers.Except(discordGuild.IgnoredCarriers))));
            mockFactory.Setup(tcwf => tcwf.GetWriterAsync(discordGuild.CarrierMovementChannel))
                       .Returns(Task.FromResult(mockTextChannelWriter.Object));
        }
        TextChannelWriterFactory factory = mockFactory.Object;

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

        // TODO: Get this to work
        mockRepository.VerifyAll();
        // mockRepository.VerifyNoOtherCalls(); // Only if we can mock ILogger
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

    private static (string, DiscordGuild, StarSystem, Carrier[], bool) NoPresence(OrderBotDbContext dbContext)
    {
        MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
        DiscordGuild testGuild = new() { Name = "Test Guild", CarrierMovementChannel = 1234567890 };
        testGuild.SupportedMinorFactions.Add(minorFaction);
        StarSystem starSystem = new() { Name = "LTT 2684" };
        dbContext.DiscordGuilds.Add(testGuild);
        dbContext.MinorFactions.Add(minorFaction);
        dbContext.StarSystems.Add(starSystem);

        Carrier[] expectedCarriers = Array.Empty<Carrier>();

        return (Ltt2684Message, testGuild, starSystem, expectedCarriers, false);
    }

    private static (string, DiscordGuild, StarSystem, Carrier[], bool) Presence(OrderBotDbContext dbContext)
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

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers, true);
    }

    private static (string, DiscordGuild, StarSystem, Carrier[], bool) Goal(OrderBotDbContext dbContext)
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

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers, true);
    }

    private static (string, DiscordGuild, StarSystem, Carrier[], bool) AllIgnored(OrderBotDbContext dbContext)
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

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers, false);
    }

    private static (string, DiscordGuild, StarSystem, Carrier[], bool) NoChannel(OrderBotDbContext dbContext)
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

        return (Ltt2684Message, testGuild, ltt2684, expectedCarriers, false);
    }
}
