using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OrderBot.EntityFramework;

namespace OrderBot.ToDo;

/// <summary>
/// Autocomplete handler for minor factions used in goals for the current Discord guild.
/// </summary>
/// <seealso cref="MinorFactionsAutocompleteHandler"/>
internal class GoalMinorFactionsAutocompleteHandler : AutocompleteHandler
{
    public GoalMinorFactionsAutocompleteHandler(OrderBotDbContext dbContext)
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
                GetMinorFactions(autocompleteInteraction.GuildId ?? 0, enteredName).Select(mfn => new AutocompleteResult(mfn, mfn))));
    }

    protected internal IEnumerable<string> GetMinorFactions(ulong guildId, string enteredName)
    {
        return DbContext.DiscordGuildPresenceGoals.Include(dgpg => dgpg.Presence)
                                                  .Include(dgpg => dgpg.Presence.MinorFaction)
                                                  .Where(dgpg => dgpg.DiscordGuild.GuildId == guildId
                                                                 && dgpg.Presence.MinorFaction.Name.StartsWith(enteredName))
                                                  .Select(dgpg => dgpg.Presence.MinorFaction.Name)
                                                  .Distinct()
                                                  .OrderBy(n => n)
                                                  .Take(SlashCommandBuilder.MaxOptionsCount);
    }
}
