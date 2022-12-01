using Discord;
using Discord.Interactions;
using System.Net;
using System.Text.Json;

namespace OrderBot.ToDo;

/// <summary>
/// Autocomplete handler for minor factions not in the database.
/// </summary>
/// <seealso cref="GoalMinorFactionsAutocompleteHandler"/>
internal class MinorFactionsAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        // See https://discordnet.dev/guides/int_framework/autocompletion.html
        string enteredName = autocompleteInteraction.Data.Current.Value.ToString() ?? "";

        JsonDocument jsonDocument;
        using (HttpClient client = new())
        {
            using Stream stream = await client.GetStreamAsync(
                $"https://elitebgs.app/api/ebgs/v5/factions?page=1&beginsWith={WebUtility.UrlEncode(enteredName)}");
            using StreamReader reader = new(stream);
            jsonDocument = JsonDocument.Parse(stream);
        }

        return AutocompletionResult.FromSuccess(
            jsonDocument.RootElement.GetProperty("docs")
                                    .EnumerateArray()
                                    .Select(je => je.GetProperty("name").GetString())
                                    .OrderBy(s => s)
                                    .Take(SlashCommandBuilder.MaxOptionsCount)
                                    .Select(s => new AutocompleteResult(s, s)));
    }
}
