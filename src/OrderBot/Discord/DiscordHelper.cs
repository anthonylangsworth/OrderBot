using Discord;
using OrderBot.Core;
using System.Transactions;

namespace OrderBot.Discord
{
    internal static class DiscordHelper
    {
        /// <summary>
        /// Get a <see cref="DiscordGuild"/> from the database, for the given <see cref="guild"/>,
        /// creating one if it does not exist.
        /// </summary>
        /// <param name="dbContext">
        /// The <see cref="OrderBotDbContext"/> reprsenting the database.
        /// </param>
        /// <param name="guild">
        /// The <see cref="IGuild"/> to get or create a <see cref="DiscordGuild"/> for.
        /// </param>
        /// <returns>
        /// The created or retrieved <see cref="DiscordGuild"/>.
        /// </returns>
        public static DiscordGuild GetOrAddGuild(OrderBotDbContext dbContext, IGuild guild, IQueryable<DiscordGuild>? discordGuilds = null)
        {
            using TransactionScope transactionScope = new();
            DiscordGuild? discordGuild = (discordGuilds ?? dbContext.DiscordGuilds).FirstOrDefault(dg => dg.GuildId == guild.Id);
            if (discordGuild == null)
            {
                discordGuild = new DiscordGuild() { GuildId = guild.Id, Name = guild.Name };
                dbContext.DiscordGuilds.Add(discordGuild);
            }
            else if (discordGuild.Name != guild.Name)
            {
                discordGuild.Name = guild.Name;
            }
            dbContext.SaveChanges();
            transactionScope.Complete();
            return discordGuild;
        }
    }
}
