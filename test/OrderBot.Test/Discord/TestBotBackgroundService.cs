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
            string guildId = "a";
            using OrderBotDbContextFactory contextFactory = new();
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            BotBackgroundService.AddDiscordGuild(contextFactory, guildId);

            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            Assert.That(dbContext.DiscordGuilds.Where(dg => dg.GuildId == guildId).Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddDiscordGuild_Existing()
        {
            string guildId = "b";
            using OrderBotDbContextFactory contextFactory = new();
            using TransactionScope transactionScope = new();

            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            dbContext.DiscordGuilds.Add(new DiscordGuild() { GuildId = guildId });
            dbContext.SaveChanges();

            BotBackgroundService.AddDiscordGuild(contextFactory, guildId);

            Assert.That(dbContext.DiscordGuilds.Where(dg => dg.GuildId == guildId).Count(), Is.EqualTo(1));
        }
    }
}
