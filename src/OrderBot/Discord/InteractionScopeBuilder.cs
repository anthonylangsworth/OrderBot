using Discord;
using Discord.WebSocket;
using System.Text;

namespace OrderBot.Discord;

/// <summary>
/// Use with <see cref="ILogger.BeginScope"/> to standardize parameters.
/// </summary>
internal class InteractionScopeBuilder : ScopeBuilder
{
    /// <summary>
    /// Create a new <see cref="InteractionScopeBuilder"/>, getting details from
    /// <paramref name="context"/>.
    /// </summary>
    /// <param name="context">
    /// Source of details about the current interaction, user and guild.
    /// </param>
    public InteractionScopeBuilder(IInteractionContext context)
    {
        Add("InteractionId", context.Interaction.Id);
        Add("Command", GetCommand(context));

        if (context.Guild != null)
        {
            Add("Guild", context.Guild.Name);
            Add("GuildId", context.Guild.Id);
            Add("User", context.Guild.GetUserAsync(context.User.Id).GetAwaiter().GetResult().DisplayName);
        }
        else
        {
            Add("User", context.User.Username);
        }
    }

    protected static string GetCommand(IInteractionContext context)
    {
        return context.Interaction switch
        {
            SocketSlashCommand socketSlashCommand => $"/{socketSlashCommand.CommandName}{GetCommandOptions(socketSlashCommand.Data.Options)}",
            IAutocompleteInteraction autocomplete => $"Autocomplete {autocomplete.Data.CommandName}{GetAutocompleteOptions(autocomplete.Data.Options)}",
            _ => "Unknown",
        };
    }

    protected static string GetCommandOptions(IEnumerable<IApplicationCommandInteractionDataOption> options)
    {
        StringBuilder result = new();
        foreach (IApplicationCommandInteractionDataOption option in options)
        {
            result.Append(" " + option.Name + (option.Value != null ? " " + option.Value : ""));
            if (option.Options.Any())
            {
                result.Append(GetCommandOptions(option.Options));
            }
        }
        return result.ToString();
    }

    protected static string GetAutocompleteOptions(IEnumerable<AutocompleteOption> options)
    {
        StringBuilder result = new();
        foreach (AutocompleteOption option in options)
        {
            result.Append(" " + option.Name + (option.Value != null ? " " + option.Value : ""));
        }
        return result.ToString();
    }
}
