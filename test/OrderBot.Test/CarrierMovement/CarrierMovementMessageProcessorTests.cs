using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.Test.ToDo;
using System.Reflection;
using System.Text;
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
        using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        IOptions<DiscordClientConfig> options = Options.Create(new DiscordClientConfig { ApiKey = "" });

        CarrierMovementMessageProcessor messageProcessor = new(dbContext,
            logger, discordClient, memoryCache, options);

        Assert.That(messageProcessor.Logger, Is.EqualTo(logger));
        Assert.That(messageProcessor.DiscordClient, Is.EqualTo(discordClient));
        Assert.That(messageProcessor.DbContext, Is.EqualTo(dbContext));
    }

    //[SetUp]
    //public void SetUp()
    //{
    //    using OrderBotDbContextFactory contextFactory = new();
    //    using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
    //    using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
    //    using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
    //}

    //[TearDown]
    //public void TearDown()
    //{
    //}

    //OrderBotDbContextFactory ContextFactory;
    //OrderBotDbContext DbContext;
    //TransactionScope TransactionScope;

    [Test]
    public void Process()
    {
        DateTime fileTimeStamp = DateTime.Parse("2022-10-30T13:56:57Z").ToUniversalTime();
        using Stream? stream = Assembly.GetExecutingAssembly()?.GetManifestResourceStream("OrderBot.Test.samples.LTT 2684 FSS.json");
        if (stream != null)
        {
            using OrderBotDbContextFactory contextFactory = new();
            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            IOptions<DiscordClientConfig> options = Options.Create(new DiscordClientConfig { ApiKey = "" });

            const ulong carrierMovementChannelId = 1234567890;
            Carrier cowboyB = new() { Name = "Cowboy B X9Z-B0B" };
            MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
            DiscordGuild testGuild = new() { Name = "Test Guild", CarrierMovementChannel = carrierMovementChannelId };
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
                new Carrier() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = fileTimeStamp },
                new Carrier() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = fileTimeStamp },
                new Carrier() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = fileTimeStamp }
            };

            MockRepository mockRepository = new(MockBehavior.Strict);
            Mock<ILogger<CarrierMovementMessageProcessor>> mockLogger =
                mockRepository.Create<ILogger<CarrierMovementMessageProcessor>>(MockBehavior.Loose);
            //mockLogger.Setup(l => l.Log(LogLevel.Information, 0,
            //    "Carrier(s) Cowboy B X9Z-B0B, T.N.V.A COSMOS HNV-L7X, E.D.A. WALKABOUT KHF-79Z, ODIN W6B-94Z in LTT 2684 updated"));
            ILogger<CarrierMovementMessageProcessor> logger = mockLogger.Object;

            IUserMessage userMessage = mockRepository.Create<IUserMessage>().Object;

            Mock<ITextChannel> mockTextChannel = mockRepository.Create<ITextChannel>();
            StringBuilder carrierMovementMessage = new();
            foreach (Carrier carrier in expectedCarriers.Except(testGuild.IgnoredCarriers).OrderBy(c => c.Name))
            {
                carrierMovementMessage.AppendLine(
                    CarrierMovementMessageProcessor.GetCarrierMovementMessage(carrier, ltt2684));
            }
            if (carrierMovementMessage.Length > 0)
            {
                mockTextChannel.Setup(tc => tc.SendMessageAsync(
                                    carrierMovementMessage.ToString(), false, null, null, null, null, null, null, null, MessageFlags.None))
                               .Returns(Task.FromResult(userMessage));
            }
            mockTextChannel.SetupGet(smc => smc.Id).Returns(carrierMovementChannelId);
            ITextChannel socketMessageChannel = mockTextChannel.Object;

            Mock<IDiscordClient> mockDiscordClient = mockRepository.Create<IDiscordClient>();
            mockDiscordClient.Setup(dc => dc.GetChannelAsync(carrierMovementChannelId, CacheMode.AllowDownload, null))
                             .ReturnsAsync(socketMessageChannel);
            mockDiscordClient.SetupGet(dc => dc.ConnectionState).Returns(ConnectionState.Connected);
            IDiscordClient discordClient = mockDiscordClient.Object;

            CarrierMovementMessageProcessor messageProcessor = new(dbContext,
                logger, discordClient, memoryCache, options);
            messageProcessor.ProcessAsync(JsonDocument.Parse(stream)).GetAwaiter().GetResult();

            Assert.That(
                dbContext.Carriers,
                Is.EquivalentTo(expectedCarriers).Using(CarrierEqualityComparer.Instance));
            mockRepository.Verify();
        }
        else
        {
            Assert.Fail("Test resource failed to load");
        }
    }
}
