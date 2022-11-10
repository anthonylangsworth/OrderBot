using Discord.Interactions;
using Discord.WebSocket;

namespace OrderBot
{
    internal class ScopeBuilder
    {
        private readonly Dictionary<string, object> _scope;

        public ScopeBuilder(SocketInteractionContext context)
        {
            _scope = new Dictionary<string, object>()
            {
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
                    result = $"{socketSlashCommand.CommandName} {string.Join(" ", socketSlashCommand.Data.Options.Select(o => o.Name))}";
                    break;

                default:
                    result = "Unknown";
                    break;
            }
            return result;
        }
    }
}
