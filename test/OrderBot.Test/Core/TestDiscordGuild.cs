using NUnit.Framework;
using OrderBot.Core;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.Test.Core
{
    internal class TestDiscordGuild
    {
        [Test]
        public void Creation_NoCarrierMovementChannel()
        {
            using OrderBotDbContextFactory orderBotDbContextFactory = new();
            using OrderBotDbContext dbContext = orderBotDbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();
            DiscordGuild guild = new() { GuildId = 1234567890 };
            dbContext.DiscordGuilds.Add(guild);
            dbContext.SaveChanges();
            DiscordGuild? loadedGuild = dbContext.DiscordGuilds.FirstOrDefault(dg => dg.GuildId == guild.GuildId);
            Assert.That(loadedGuild, Is.EqualTo(guild));
        }

        [Test]
        public void Creation_CarrierMovementChannel()
        {
            using OrderBotDbContextFactory orderBotDbContextFactory = new();
            using OrderBotDbContext dbContext = orderBotDbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();
            DiscordGuild guild = new() { GuildId = 1234567890, CarrierMovementChannel = 9876543210 };
            dbContext.DiscordGuilds.Add(guild);
            dbContext.SaveChanges();
            DiscordGuild? loadedGuild = dbContext.DiscordGuilds.FirstOrDefault(dg => dg.GuildId == guild.GuildId);
            Assert.That(loadedGuild, Is.EqualTo(guild));
        }
    }
}
