using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OrderBot.Core;
using OrderBot.EntityFramework;

namespace OrderBot.CarrierMovement
{
    public class NotIgnoredCarriersAutocompleteHandler : AutocompleteHandler
    {
        public NotIgnoredCarriersAutocompleteHandler(IDbContextFactory<OrderBotDbContext> dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
        }

        public IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            // See https://discordnet.dev/guides/int_framework/autocompletion.html
            string enteredName = autocompleteInteraction.Data.Current.Value.ToString() ?? "";

            // max - 25 suggestions at a time (API limit)
            using OrderBotDbContext dbContext = await DbContextFactory.CreateDbContextAsync();
            return AutocompletionResult.FromSuccess(
                Implementation(dbContext, enteredName, context.Guild.Id).Select(c => new AutocompleteResult(c, c))
                                                                        .Take(25)
            );
        }

        internal static IEnumerable<string> Implementation(OrderBotDbContext dbContext,
            string nameStartsWith, ulong guildId)
        {
            DiscordGuild discordGuild =
                dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                       .First(dg => dg.GuildId == guildId);
            return dbContext.Carriers.OrderBy(c => c.Name)
                                     .Where(c => !discordGuild.IgnoredCarriers.Contains(c) && c.Name.StartsWith(nameStartsWith))
                                     .Select(c => c.Name)
                                     .ToList();
        }
    }
}
