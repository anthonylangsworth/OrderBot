using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace OrderBot.Discord
{
    internal class ScopeBuilder
    {
        private readonly Dictionary<string, object> _scope;

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

        public ScopeBuilder Add(string name, object value)
        {
            _scope[name] = value;
            return this;
        }

        public IReadOnlyCollection<KeyValuePair<string, object>> Build()
        {
            return _scope;
        }

        public string GetCommand(SocketInteractionContext context)
        {
            return context.Interaction switch
            {
                SocketSlashCommand socketSlashCommand => $"/{socketSlashCommand.CommandName}{GetCommandOptions(socketSlashCommand.Data.Options)}",
                SocketAutocompleteInteraction autocomplete => $"Autocomplete {autocomplete.Data.CommandName}{GetAutocompleteOptions(autocomplete.Data.Options)}",
                _ => "Unknown",
            };
        }

        public string GetCommandOptions(IEnumerable<IApplicationCommandInteractionDataOption> options)
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

        public string GetAutocompleteOptions(IEnumerable<AutocompleteOption> options)
        {
            StringBuilder result = new();
            foreach (AutocompleteOption option in options)
            {
                result.Append(" " + option.Name + (option.Value != null ? " " + option.Value : ""));
            }
            return result.ToString();
        }
    }
}
