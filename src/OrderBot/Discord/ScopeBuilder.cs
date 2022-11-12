using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace OrderBot.Discord;

/// <summary>
/// Use with <see cref="ILogger.BeginScope"/> to standardize parameters.
/// </summary>
internal class ScopeBuilder
{
    private readonly Dictionary<string, object> _scope;

    /// <summary>
    /// Create a new <see cref="ScopeBuilder"/>, getting details from
    /// <paramref name="context"/>.
    /// </summary>
    /// <param name="context">
    /// Source of details about the current interaction, user and guild.
    /// </param>
    public ScopeBuilder(SocketInteractionContext context)
    {
        _scope = new Dictionary<string, object>()
        {
            { "InteractionId", context.Interaction.Id },
            { "Command", GetCommand(context) },
        };

        if (context.Guild != null)
        {
            _scope["Guild"] = context.Guild.Name;
            _scope["User"] = context.Guild.GetUser(context.User.Id).DisplayName;
        }
        else
        {
            _scope["User"] = context.User.Username;
        }
    }

    /// <summary>
    /// Add new parameter.
    /// </summary>
    /// <param name="name">
    /// The name. Should should be in Pascal case.
    /// </param>
    /// <param name="value">
    /// The value. Should have a human-readable ToString().
    /// </param>
    /// <returns>
    /// This object for fluent use.
    /// </returns>
    public ScopeBuilder Add(string name, object value)
    {
        _scope[name] = value;
        return this;
    }

    /// <summary>
    /// Construct the parameters to pass to <see cref="ILogger.BeginScope"/>.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<KeyValuePair<string, object>> Build()
    {
        return _scope;
    }

    protected string GetCommand(SocketInteractionContext context)
    {
        return context.Interaction switch
        {
            SocketSlashCommand socketSlashCommand => $"/{socketSlashCommand.CommandName}{GetCommandOptions(socketSlashCommand.Data.Options)}",
            SocketAutocompleteInteraction autocomplete => $"Autocomplete {autocomplete.Data.CommandName}{GetAutocompleteOptions(autocomplete.Data.Options)}",
            _ => "Unknown",
        };
    }

    protected string GetCommandOptions(IEnumerable<IApplicationCommandInteractionDataOption> options)
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

    protected string GetAutocompleteOptions(IEnumerable<AutocompleteOption> options)
    {
        StringBuilder result = new();
        foreach (AutocompleteOption option in options)
        {
            result.Append(" " + option.Name + (option.Value != null ? " " + option.Value : ""));
        }
        return result.ToString();
    }
}
