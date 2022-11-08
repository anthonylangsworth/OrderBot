using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OrderBot.Core;
using OrderBot.EntityFramework;

namespace OrderBot.CarrierMovement
{
    /// <summary>
    /// Nase class for autocomplete handlers dealing with carriers.
    /// </summary>
    public abstract class CarriersAutocompleteHandler : AutocompleteHandler
    {
        /// <summary>
        /// Create a new <see cref="CarriersAutocompleteHandler"/>.
        /// </summary>
        /// <param name="dbContextFactory">
        /// The database to check.
        /// </param>
        protected CarriersAutocompleteHandler(IDbContextFactory<OrderBotDbContext> dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// The database to check.
        /// </summary>
        public IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }

        /// <summary>
        /// Return up to <see cref="SlashCommandBuilder.MaxOptionsCount"/> carriers
        /// from <see cref="OrderBotDbContext.Carriers"/> matching <see cref="carrierFilter"/>
        /// converted to a <see cref="AutocompleteResult"/>.
        /// </summary>
        /// <param name="dbContext">
        /// The <see cref="OrderBotDbContext"/> to load carriers from.
        /// </param>
        /// <param name="carrierFilter">
        /// Return carriers matching this predicate.
        /// </param>
        /// <returns>
        /// Up to <see cref="SlashCommandBuilder.MaxOptionsCount"/> matching carriers.
        /// </returns>
        protected internal abstract IEnumerable<string> GetCarriers(OrderBotDbContext dbContext,
            DiscordGuild discordGuild, string startsWith);

        /// <inheritdoc/>
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            // See https://discordnet.dev/guides/int_framework/autocompletion.html
            string enteredName = autocompleteInteraction.Data.Current.Value.ToString() ?? "";

            using OrderBotDbContext dbContext = await DbContextFactory.CreateDbContextAsync();
            DiscordGuild discordGuild =
                dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                       .First(dg => dg.GuildId == context.Guild.Id);
            return AutocompletionResult.FromSuccess(
                GetCarriers(dbContext, discordGuild, enteredName).Select(c => new AutocompleteResult(c, c))

            );
        }
    }
}
