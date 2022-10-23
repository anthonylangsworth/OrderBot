using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Discord;
using System.Transactions;

namespace OrderBot.Test.Discord
{
    internal class TestBotBackgroundService
    {
        [Test]
        public void AddDiscordGuild_None()
        {
            ulong guildId = 993002946415558726;
            using OrderBotDbContextFactory contextFactory = new();
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            BotHostedService.AddDiscordGuild(contextFactory, guildId);

            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            Assert.That(dbContext.DiscordGuilds.Where(dg => dg.GuildId == guildId).Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddDiscordGuild_Existing()
        {
            ulong guildId = 993202956415658726;
            using OrderBotDbContextFactory contextFactory = new();
            using TransactionScope transactionScope = new();

            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            dbContext.DiscordGuilds.Add(new DiscordGuild() { GuildId = guildId });
            dbContext.SaveChanges();

            BotHostedService.AddDiscordGuild(contextFactory, guildId);

            Assert.That(dbContext.DiscordGuilds.Where(dg => dg.GuildId == guildId).Count(), Is.EqualTo(1));
        }
    }
}
