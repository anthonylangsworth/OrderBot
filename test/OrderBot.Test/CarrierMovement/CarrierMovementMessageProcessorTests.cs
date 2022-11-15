using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
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
        ILogger<CarrierMovementMessageProcessor> logger = NullLogger<CarrierMovementMessageProcessor>.Instance;
        IDiscordClient discordClient = Mock.Of<IDiscordClient>();
        using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

        CarrierMovementMessageProcessor messageProcessor = new(contextFactory,
            logger, discordClient, memoryCache);

        Assert.That(messageProcessor.Logger, Is.EqualTo(logger));
        Assert.That(messageProcessor.DiscordClient, Is.EqualTo(discordClient));
        Assert.That(messageProcessor.ContextFactory, Is.EqualTo(contextFactory));
    }

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
            ILogger<CarrierMovementMessageProcessor> logger = mockRepository.Create<ILogger<CarrierMovementMessageProcessor>>().Object;

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
            IDiscordClient discordClient = mockDiscordClient.Object;

            CarrierMovementMessageProcessor messageProcessor = new(contextFactory,
                logger, discordClient, memoryCache);
            messageProcessor.Process(JsonDocument.Parse(stream));

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
