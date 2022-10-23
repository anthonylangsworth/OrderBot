using Discord;
using Moq;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Discord;
using System.Transactions;

namespace OrderBot.Test.Discord
{
    internal class TestDiscordHelper
    {
        [Test]
        public void DiscordHelper_New()
        {
            const ulong guildId = 1234567890;
            const string guildName = "Test Guild Name";

            using OrderBotDbContextFactory orderBotDbContextFactory = new();
            using OrderBotDbContext dbContext = orderBotDbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            Assert.That(dbContext.DiscordGuilds.FirstOrDefault(dg => dg.GuildId == guildId), Is.Null);

            IGuild guild = Mock.Of<IGuild>(g => g.Id == guildId
                                                && g.Name == guildName);
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild);

            Assert.That(discordGuild.GuildId, Is.EqualTo(guildId));
            Assert.That(discordGuild.Name, Is.EqualTo(guildName));
            Assert.That(dbContext.DiscordGuilds.FirstOrDefault(dg => dg.GuildId == guildId),
                Is.Not.Null.And.Property("GuildId").EqualTo(guildId).And.Property("Name").EqualTo(guildName));
        }

        [Test]
        public void DiscordHelper_Exists()
        {
            const ulong guildId = 9876543210;
            const string guildName = "Other Guild Name";

            using OrderBotDbContextFactory orderBotDbContextFactory = new();
            using OrderBotDbContext dbContext = orderBotDbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            dbContext.DiscordGuilds.Add(new DiscordGuild() { GuildId = guildId, Name = guildName });
            dbContext.SaveChanges();

            IGuild guild = Mock.Of<IGuild>(g => g.Id == guildId
                                                && g.Name == guildName);
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild);

            Assert.That(discordGuild.GuildId, Is.EqualTo(guildId));
            Assert.That(discordGuild.Name, Is.EqualTo(guildName));
            Assert.That(dbContext.DiscordGuilds.Count(dg => dg.GuildId == guildId), Is.EqualTo(1));
        }

        [Test]
        public void DiscordHelper_Exists_Renamed()
        {
            const ulong guildId = 9876543210;
            const string oldGuildName = "Old Guild Name";
            const string newGuildName = "Name Guild Name";

            using OrderBotDbContextFactory orderBotDbContextFactory = new();
            using OrderBotDbContext dbContext = orderBotDbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            dbContext.DiscordGuilds.Add(new DiscordGuild() { GuildId = guildId, Name = oldGuildName });
            dbContext.SaveChanges();

            IGuild guild = Mock.Of<IGuild>(g => g.Id == guildId
                                                && g.Name == newGuildName);
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild);

            Assert.That(discordGuild.GuildId, Is.EqualTo(guildId));
            Assert.That(discordGuild.Name, Is.EqualTo(newGuildName));
            Assert.That(dbContext.DiscordGuilds.Count(dg => dg.GuildId == guildId), Is.EqualTo(1));
        }
    }
}
