using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderBot.Core;
using OrderBot.EntityFramework;

namespace OrderBot.Discord;

/// <summary>
/// Require the user to be a member of a role.
/// </summary>
internal class RequireRoleAttribute : PreconditionAttribute
{
    /// <summary>
    /// Create a new <see cref="RequireRoleAttribute"/>.
    /// </summary>
    /// <param name="roleName">
    /// The role name.
    /// </param>
    public RequireRoleAttribute(string roleName)
    {
        RoleName = roleName;
    }

    /// <summary>
    /// The role name.
    /// </summary>
    public string RoleName { get; }

    /// <inheritdoc/>
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo,
        IServiceProvider services)
    {
        IDbContextFactory<OrderBotDbContext> contextFactory = services.GetRequiredService<IDbContextFactory<OrderBotDbContext>>();
        using OrderBotDbContext dbContext = await contextFactory.CreateDbContextAsync();

        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, context.Guild);
        IList<ulong> roleIds = dbContext.RoleMembers.Where(rm => rm.DiscordGuild == discordGuild
                                                              && rm.Role.Name == RoleName)
                                                    .Select(rm => rm.MentionableId)
                                                    .ToList();
        if ((await context.Guild.GetUserAsync(context.User.Id)).RoleIds.Intersect(roleIds).Any())
        {
            return PreconditionResult.FromSuccess();
        }
        else
        {
            return PreconditionResult.FromError($"You are not in the required role(s).");
        }
    }
}
