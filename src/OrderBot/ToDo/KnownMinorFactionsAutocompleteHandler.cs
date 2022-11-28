using Discord;
using Discord.Interactions;
using OrderBot.EntityFramework;

namespace OrderBot.ToDo;

/// <summary>
/// Autocomplete handler for known minor factions.
/// </summary>
internal class KnownMinorFactionsAutocompleteHandler : AutocompleteHandler
{
    public KnownMinorFactionsAutocompleteHandler(OrderBotDbContext dbContext)
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
                DbContext.MinorFactions.Where(mf => mf.Name.StartsWith(enteredName))
                                       .OrderBy(mf => mf.Name)
                                       .Take(SlashCommandBuilder.MaxOptionsCount)
                                       .Select(mf => new AutocompleteResult(mf.Name, mf.Name))));
    }
}
