using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace OrderBot.Discord
{
    // TODO: Change this to load the required roles for a Guild from the database

    internal class RequirePerGuildRoleAttribute : PreconditionAttribute
    {
        public RequirePerGuildRoleAttribute(params string[] roles)
        {
            Roles = roles.ToArray();
        }
        public IReadOnlyList<string> Roles { get; }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo,
            IServiceProvider services)
        {
            if (context.User is SocketGuildUser socketGuildUser)
            {
                if (Roles.All(role => socketGuildUser.Roles.Any(r => r.Name == role)))
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
                else
                {
                    return Task.FromResult(PreconditionResult.FromError($"You lack permission to run this command."));
                }
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
            }
        }
    }
}
