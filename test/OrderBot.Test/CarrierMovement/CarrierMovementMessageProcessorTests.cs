using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.EntityFramework;
using OrderBot.Test.ToDo;
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
            IDiscordClient discordClient = Mock.Of<IDiscordClient>();

            Carrier cowboyB = new() { Name = "Cowboy B X9Z-B0B" };
            MinorFaction minorFaction = new() { Name = "Test Minor Faction" };
            DiscordGuild testGuild = new() { Name = "Test Guild" };
            testGuild.IgnoredCarriers.Add(cowboyB);
            testGuild.SupportedMinorFactions.Add(minorFaction);
            StarSystem ltt2684 = new() { Name = "LTT 2684" };
            dbContext.Carriers.Add(cowboyB);
            dbContext.DiscordGuilds.Add(testGuild);
            dbContext.MinorFactions.Add(minorFaction);
            dbContext.StarSystems.Add(ltt2684);
            dbContext.SaveChanges();

            CarrierMovementMessageProcessor messageProcessor = new(contextFactory,
                logger, discordClient);
            messageProcessor.Process(JsonDocument.Parse(stream));

            Assert.That(
                dbContext.Carriers,
                Is.EquivalentTo(new Carrier[]
                {
                    new Carrier() { Name = "Cowboy B X9Z-B0B" }, // Ignored, so should not update
                    new Carrier() { Name = "T.N.V.A COSMOS HNV-L7X", StarSystem = ltt2684, FirstSeen = fileTimeStamp},
                    new Carrier() { Name = "E.D.A. WALKABOUT KHF-79Z", StarSystem = ltt2684, FirstSeen = fileTimeStamp },
                    new Carrier() { Name = "ODIN W6B-94Z", StarSystem = ltt2684, FirstSeen = fileTimeStamp }
                }).Using(CarrierEqualityComparer.Instance));
        }
        else
        {
            Assert.Fail("Test resource failed to load");
        }

    }
}
