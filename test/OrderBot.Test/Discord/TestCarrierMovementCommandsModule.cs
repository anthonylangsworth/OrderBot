using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Discord;
using System.Transactions;

namespace OrderBot.Test.Discord
{
    internal class TestCarrierMovementCommandsModule
    {
        [Test]
        [Ignore("Cannot mock Discord.Net")]
        public async Task ListIgnoredCarriers()
        {
            string[] ignoreCarriers =
            {
                "Alpha A2R-GHN",
                "Omega O7U-44F",
                "Delta VT4-GKW",
            };

            using OrderBotDbContextFactory contextFactory = new();
            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            DiscordGuild discordGuild = new() { GuildId = 1234567890, Name = "Test Guild" };
            dbContext.DiscordGuilds.Add(discordGuild);
            dbContext.SaveChanges();

            foreach (string ignoreCarrier in ignoreCarriers)
            {
                MockRepository mockRepository = new(MockBehavior.Default);
                DiscordSocketClient discordSocketClient = new();
                Mock<SocketSlashCommand> mockSocketSlashCommand = mockRepository.Create<SocketSlashCommand>(discordSocketClient, null, null, null);
                mockSocketSlashCommand.Setup(si => si.DeferAsync(true, null));
                mockSocketSlashCommand.Setup(si => si.RespondAsync($"Fleet carrier '{ignoreCarrier}' will **NOT** be tracked or its location reported", null, false, true, null, null, null, null));
                SocketSlashCommand socketSlashCommand = mockSocketSlashCommand.Object;
                SocketInteractionContext socketInteractionContext = new(discordSocketClient, null);

                CarrierMovementCommandsModule module = new(contextFactory, new NullLogger<CarrierMovementCommandsModule>());
                ((IInteractionModuleBase)module).SetContext(socketInteractionContext);
                await module.IgnoreCarrier(ignoreCarrier);

                mockRepository.VerifyAll();
                Assert.That(() => dbContext.Carriers.First(c => c.Name == ignoreCarrier), Throws.Nothing);
            }
        }
    }
}
