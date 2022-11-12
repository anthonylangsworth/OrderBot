using Discord;
using Discord.Interactions;

namespace OrderBot.ToDo;

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
                Goals.Map.Values
                     .OrderBy(g => g.Name)
                     .Where(g => g.Name.StartsWith(enteredGoal, StringComparison.OrdinalIgnoreCase))
                     .Select(g => new AutocompleteResult($"{g.Name} ({g.Description})", g.Name))
                     .Take(SlashCommandBuilder.MaxOptionsCount)
                     .ToList()));
    }
}
