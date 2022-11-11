using Discord;
using Discord.Interactions;

namespace OrderBot.Admin
{
    /// <summary>
    /// Autocomplete handler for role names.
    /// </summary>
    public class RolesAutocompleteHandler : AutocompleteHandler
    {
        /// <inheritdoc/>
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            // See https://discordnet.dev/guides/int_framework/autocompletion.html
            string enteredGoal = autocompleteInteraction.Data.Current.Value.ToString() ?? "";

            return Task.FromResult(
                AutocompletionResult.FromSuccess(
                    Roles.Map.Values
                         .OrderBy(r => r.Name)
                         .Where(r => r.Name.StartsWith(enteredGoal, StringComparison.OrdinalIgnoreCase))
                         .Select(r => new AutocompleteResult($"{r.Name} ({r.Description})", r.Name))
                         .Take(SlashCommandBuilder.MaxOptionsCount)
                         .ToList()));
        }
    }
}
