using Discord;
using Discord.Interactions;
using OrderBot.EntityFramework;

namespace OrderBot.ToDo;

/// <summary>
/// Autocomplete handler for known star systems.
/// </summary>
internal class KnownStarSystemsAutocompleteHandler : AutocompleteHandler
{
    public KnownStarSystemsAutocompleteHandler(OrderBotDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public OrderBotDbContext DbContext { get; }

    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        // See https://discordnet.dev/guides/int_framework/autocompletion.html
        string enteredName = autocompleteInteraction.Data.Current.Value.ToString() ?? "";

        return Task.FromResult(
            AutocompletionResult.FromSuccess(
                DbContext.StarSystems.Where(ss => ss.Name.StartsWith(enteredName))
                                     .OrderBy(ss => ss.Name)
                                     .Take(SlashCommandBuilder.MaxOptionsCount)
                                     .Select(ss => new AutocompleteResult(ss.Name, ss.Name))));
    }
}
