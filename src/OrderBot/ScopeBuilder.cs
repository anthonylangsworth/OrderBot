using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace OrderBot
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
            string result;
            switch (context.Interaction)
            {
                case SocketSlashCommand socketSlashCommand:
                    // result = $"/{socketSlashCommand.CommandName} {string.Join(" ", socketSlashCommand.Data.Options.Select(o => o.Name + (o.Value != null ? " " + o.Value : "")))}";
                    result = $"/{socketSlashCommand.CommandName}{GetCommandOptions(socketSlashCommand.Data.Options)}";
                    break;

                case SocketAutocompleteInteraction autocomplete:
                    // result = $"Autocomplete {autocomplete.Data.CommandName} {string.Join(" ", autocomplete.Data.Options.Select(o => o.Name + (o.Value != null ? " " + o.Value : "")))}";
                    result = $"Autocomplete {autocomplete.Data.CommandName}{GetAutocompleteOptions(autocomplete.Data.Options)}";
                    break;

                default:
                    result = "Unknown";
                    break;
            }
            return result;
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
