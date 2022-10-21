using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OrderBot.Core;

namespace OrderBot.CarrierMovement
{
    public class TrackedCarriersAutocompleteHandler : AutocompleteHandler
    {
        public TrackedCarriersAutocompleteHandler(IDbContextFactory<OrderBotDbContext> dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
        }

        public IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            // See https://discordnet.dev/guides/int_framework/autocompletion.html

            string currentValue = autocompleteInteraction.Data.Current.Value.ToString() ?? "";
            using OrderBotDbContext dbContext = await DbContextFactory.CreateDbContextAsync();
            DiscordGuild? discordGuild = dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                                                .FirstOrDefault(dg => dg.GuildId == context.Guild.Id);
            IList<AutocompleteResult> carrierNames;
            carrierNames = (discordGuild != null ? dbContext.Carriers.Except(discordGuild.IgnoredCarriers) : dbContext.Carriers)
                                .Where(c => c.Name.StartsWith(currentValue))
                                .OrderBy(c => c.Name)
                                .Select(c => new AutocompleteResult(c.Name, c.Name))
                                .ToList();

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(carrierNames.Take(25));
        }
    }
}
