using Discord;
using Discord.Interactions;

namespace OrderBot.ToDo
{
    /// <summary>
    /// Autocomplete handler for goal names.
    /// </summary>
    public class GoalsAutocompleteHandler : AutocompleteHandler
    {
        /// <inheritdoc/>
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            // See https://discordnet.dev/guides/int_framework/autocompletion.html
            string enteredGoal = autocompleteInteraction.Data.Current.Value.ToString() ?? "";

            return Task.FromResult(
                AutocompletionResult.FromSuccess(
                    Goals.Map.Keys
                         .OrderBy(gn => gn)
                         .Where(gn => gn.StartsWith(enteredGoal, StringComparison.OrdinalIgnoreCase))
                         .Select(gn => new AutocompleteResult(gn, gn))
                         .Take(SlashCommandBuilder.MaxOptionsCount)
                         .ToList()));
        }
    }
}
