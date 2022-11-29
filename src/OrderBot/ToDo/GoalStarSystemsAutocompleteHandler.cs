using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OrderBot.EntityFramework;

namespace OrderBot.ToDo;

/// <summary>
/// Autocomplete handler for star systems used in goals for the current Discord guild.
/// </summary>
/// <seealso cref="StarSystemsAutocompleteHandler"/>
internal class GoalStarSystemsAutocompleteHandler : AutocompleteHandler
{
    public GoalStarSystemsAutocompleteHandler(OrderBotDbContext dbContext)
    {
        DbContext = dbContext;
    }

    protected OrderBotDbContext DbContext { get; }

    /// <inheritdoc/>
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        // See https://discordnet.dev/guides/int_framework/autocompletion.html
        string enteredName = autocompleteInteraction.Data.Current.Value.ToString() ?? "";

        return Task.FromResult(
            AutocompletionResult.FromSuccess(
                GetStarSystems(autocompleteInteraction.GuildId ?? 0, enteredName).Select(ssn => new AutocompleteResult(ssn, ssn))));
    }

    protected internal IEnumerable<string> GetStarSystems(ulong guildId, string enteredName)
    {
        return DbContext.DiscordGuildPresenceGoals.Include(dgpg => dgpg.Presence)
                                                  .Include(dgpg => dgpg.Presence.StarSystem)
                                                  .Where(dgpg => dgpg.DiscordGuild.GuildId == guildId
                                                                 && dgpg.Presence.StarSystem.Name.StartsWith(enteredName))
                                                  .Select(dgpg => dgpg.Presence.StarSystem.Name)
                                                  .Distinct()
                                                  .OrderBy(n => n)
                                                  .Take(SlashCommandBuilder.MaxOptionsCount);
    }
}
