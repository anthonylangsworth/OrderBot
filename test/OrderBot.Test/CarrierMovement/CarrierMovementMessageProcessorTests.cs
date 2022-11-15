using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.EntityFramework;
using OrderBot.Test.ToDo;
using System.Net;
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
        ILogger<CarrierMovementMessageProcessor> logger = NullLogger<CarrierMovementMessageProcessor>.Instance;
        IDiscordClient discordClient = Mock.Of<IDiscordClient>();

        CarrierMovementMessageProcessor messageProcessor = new(contextFactory,
            logger, discordClient);

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
            ILogger<CarrierMovementMessageProcessor> logger = NullLogger<CarrierMovementMessageProcessor>.Instance;

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
            dbContext.SaveChanges();

            Carrier[] expectedCarriers = new Carrier[]
            {
                cowboyB, // Ignored, so should not update
                new Carrier() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = fileTimeStamp},
                new Carrier() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = fileTimeStamp },
                new Carrier() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = fileTimeStamp }
            };

            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);
            Mock<ISocketMessageChannel> mockMessageChannel = mockRepository.Create<ISocketMessageChannel>();
            foreach (Carrier carrier in expectedCarriers.Where(c => !testGuild.IgnoredCarriers.Contains(c)))
            {
                mockMessageChannel.Setup(smc => smc.SendMessageAsync(
                    $"New fleet carrier '{carrier.Name}'(<https://inara.cz/elite/search/?search={WebUtility.UrlEncode(carrier.SerialNumber)}>) seen in '{ltt2684.Name}'(<https://inara.cz/elite/search/?search={WebUtility.UrlEncode(ltt2684.Name)}>).",
                    false, null, null, null, null, null, null, null, MessageFlags.None));
            }
            mockMessageChannel.SetupGet(smc => smc.Id).Returns(carrierMovementChannelId);
            ISocketMessageChannel socketMessageChannel = mockMessageChannel.Object;
            Mock<IDiscordClient> mockDiscordClient = mockRepository.Create<IDiscordClient>();
            mockDiscordClient.Setup(dc => dc.GetChannelAsync(carrierMovementChannelId, CacheMode.AllowDownload, null))
                             .ReturnsAsync(socketMessageChannel);
            IDiscordClient discordClient = mockDiscordClient.Object;

            CarrierMovementMessageProcessor messageProcessor = new(contextFactory,
                logger, discordClient);
            messageProcessor.Process(JsonDocument.Parse(stream));

            Assert.That(
                dbContext.Carriers,
                Is.EquivalentTo(expectedCarriers).Using(CarrierEqualityComparer.Instance));
            mockRepository.VerifyAll();
            mockRepository.VerifyNoOtherCalls();
        }
        else
        {
            Assert.Fail("Test resource failed to load");
        }
    }
}
