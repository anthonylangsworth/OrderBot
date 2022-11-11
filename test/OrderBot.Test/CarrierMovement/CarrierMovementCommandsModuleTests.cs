using Discord;
using Moq;
using NUnit.Framework;
using OrderBot.Admin;
using OrderBot.CarrierMovement;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.Test.CarrierMovement
{
    internal class CarrierMovementCommandsModuleTests
    {
        [Test]
        public void IgnoreCarriers()
        {
            string[] ignoreCarriers =
            {
                "Alpha A2R-GHN",
                "Omega O7U-44F",
                "Delta VT4-GKW",
            };

            NullAuditLogger nullDiscordAuditLog = new();
            using OrderBotDbContextFactory contextFactory = new();
            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            IGuild guild = Mock.Of<IGuild>(g => g.Id == 1234567890 && g.Name == "Test Guild");

            Assert.That(
                CarrierMovementCommandsModule.IgnoredCarriers.ListImplementation(dbContext, guild).Select(c => c.Name),
                Is.Empty);

            foreach (string ignoreCarrier in ignoreCarriers)
            {
                CarrierMovementCommandsModule.IgnoredCarriers.AddImplementation(dbContext, guild,
                    new[] { ignoreCarrier });
                Assert.That(
                    CarrierMovementCommandsModule.IgnoredCarriers.ListImplementation(dbContext, guild)
                                                                 .Any(c => c.Name == ignoreCarrier), Is.True);
            }

            Assert.That(
                CarrierMovementCommandsModule.IgnoredCarriers.ListImplementation(dbContext, guild).Select(c => c.Name),
                Is.EqualTo(ignoreCarriers.OrderBy(s => s)));

            foreach (string ignoreCarrier in ignoreCarriers)
            {
                CarrierMovementCommandsModule.IgnoredCarriers.RemoveImplementation(dbContext, guild,
                    ignoreCarrier);
                Assert.That(
                    CarrierMovementCommandsModule.IgnoredCarriers.ListImplementation(dbContext, guild)
                                                                 .All(c => c.Name != ignoreCarrier), Is.True);
            }

            Assert.That(
                CarrierMovementCommandsModule.IgnoredCarriers.ListImplementation(dbContext, guild).Select(c => c.Name),
                Is.Empty);
        }
    }
}
