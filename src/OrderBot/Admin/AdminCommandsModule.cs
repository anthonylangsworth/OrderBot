using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.Rbac;

namespace OrderBot.Admin;

[Group("bgs-order-bot", "BGS Order Bot Admin commands")]
[RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
public class AdminCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    [Group("audit-channel", "Audit events")]
    public class AuditChannel : BaseCommandsModule<AuditChannel>
    {
        /// <summary>
        /// Create a new <see cref="AuditChannel"/>.
        /// </summary>
        /// <param name="dbContext">
        /// </param>
        /// <param name="logger">
        /// A <see cref="Logger{AuditChannel}"/> to log to.
        /// </param>
        /// <param name="auditLogFactory">
        /// </param>
        /// <param name="resultFactory">
        /// </param>
        public AuditChannel(OrderBotDbContext dbContext, ILogger<AuditChannel> logger,
            TextChannelAuditLoggerFactory auditLogFactory, ResultFactory resultFactory)
            : base(dbContext, logger, auditLogFactory, resultFactory)
        {
            // Do nothing
        }

        [SlashCommand("set", "Change the channel to which audit messages are written")]
        public async Task Set(
            [Summary("Channel", "Write audit messages to this chanel")]
            IChannel channel
        )
        {
            try
            {
                string errorMessage = null!;
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);

                if (Context.Guild.GetChannel(channel.Id) is SocketTextChannel newAuditChannel
                    && newAuditChannel != null)
                {
                    SocketTextChannel? oldAuditChannel = Context.Guild.GetChannel(discordGuild.AuditChannel ?? 0) as SocketTextChannel;
                    string auditMessage =
                        $"{Context.User.Username} changed the BGS Order Bot audit channel from {oldAuditChannel?.Mention ?? "(None)"} to {newAuditChannel?.Mention}";
                    try
                    {
                        if (oldAuditChannel != null)
                        {
                            await oldAuditChannel.SendMessageAsync(auditMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Cannot write to old audit channel {channelName}. Ensure the bot has 'Send Messages' permission.",
                            oldAuditChannel?.Name ?? oldAuditChannel?.Id.ToString());
                    }

                    try
                    {
#pragma warning disable CS8602
                        await newAuditChannel.SendMessageAsync(auditMessage);
#pragma warning restore CS8602
                    }
                    catch (Exception)
                    {
                        errorMessage = $"Cannot write to new audit channel {newAuditChannel?.Mention}. Ensure the bot has 'Send Messages' permission.";
                    }
                }
                else
                {
                    errorMessage = $"{MentionUtils.MentionChannel(channel.Id)} must be a text channel";
                }

                if (errorMessage != null)
                {
                    await Result.Error(
                        $"Cannot set the audit channel to {MentionUtils.MentionChannel(channel?.Id ?? 0)}",
                        errorMessage,
                        "Select a text channel that the bot has 'Send Messages' permission to.");
                }
                else
                {
                    discordGuild.AuditChannel = channel?.Id ?? 0;
                    await DbContext.SaveChangesAsync();
                    await Result.Success($"Audit messages will now be written to {MentionUtils.MentionChannel(channel?.Id ?? 0)}.");
                }
                TransactionScope.Complete();
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }

        [SlashCommand("get", "Get the channel to which audit log messages are written")]
        public async Task Get()
        {
            try
            {
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);
                string message;
                if (discordGuild.AuditChannel == null)
                {
                    message = $"No audit channel set";
                }
                else
                {
                    IChannel channel = Context.Guild.GetChannel(discordGuild.AuditChannel ?? 0);
                    message = $"Audit messages will be written to {MentionUtils.MentionChannel(channel.Id)}";
                }
                await Result.Information(message);
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }

        [SlashCommand("clear", "Turn off auditing")]
        public async Task Clear()
        {
            try
            {
                // Audit here as the last entry to the audit log
                AuditLogger.Audit("Auditing disabled");
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);
                if (discordGuild.AuditChannel != null)
                {
                    discordGuild.AuditChannel = null;
                    await DbContext.SaveChangesAsync();
                }
                await Result.Success("Auditing disabled");
                TransactionScope.Complete();
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }
    }

    [Group("rbac", "Role-based Access Control")]
    public class Rbac : BaseCommandsModule<Rbac>
    {
        /// <summary>
        /// Create a new <see cref="Audit"/>.
        /// </summary>
        /// <param name="dbContext">
        /// </param>
        /// <param name="logger">
        /// A <see cref="Logger{Rbac}"/> to log to.
        /// </param>
        /// <param name="auditLogFactory">
        /// </param>
        /// <param name="resultFactory">
        /// </param>
        public Rbac(OrderBotDbContext dbContext, ILogger<Rbac> logger,
            TextChannelAuditLoggerFactory auditLogFactory, ResultFactory resultFactory)
            : base(dbContext, logger, auditLogFactory, resultFactory)
        {
            // Do nothing
        }

        [SlashCommand("add-member", "Add a member to a role")]
        public async Task AddMember(
            [
                Summary("bot-role", "The Discord bot role to add the member to"),
                Autocomplete(typeof(RolesAutocompleteHandler))
            ]
            string roleName,
            [Summary("discord-role", "The role to add")]
            IRole discordRole
        )
        {
            try
            {

                string errorMessage = null!;
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);

                if (!Roles.Map.Keys.Contains(roleName))
                {
                    errorMessage = $"Unknown role {roleName}";
                    Logger.LogWarning("Unknown role { RoleName }", roleName);
                }
                else
                {
                    Core.Role? role = DbContext.Roles.FirstOrDefault(r => r.Name == roleName);
                    if (role == null)
                    {
                        role = new Core.Role { Name = roleName };
                        DbContext.Roles.Add(role);
                    }

                    RoleMember? roleMamber =
                        DbContext.RoleMembers.FirstOrDefault(rm => rm.DiscordGuild == discordGuild
                                                                && rm.Role == role
                                                                && rm.MentionableId == discordRole.Id);
                    if (roleMamber == null)
                    {
                        roleMamber = new RoleMember
                        {
                            DiscordGuild = discordGuild,
                            Role = role,
                            MentionableId = discordRole.Id
                        };
                        DbContext.RoleMembers.Add(roleMamber);
                    }

                    DbContext.SaveChanges();
                }

                if (errorMessage == null)
                {
                    await Result.Success(
                        $"Added {MentionUtils.MentionRole(discordRole.Id)} to role '{roleName}'.", true);
                }
                else
                {
                    await Result.Error(
                        $"Cannot add {MentionUtils.MentionRole(discordRole.Id)} to role '{roleName}.",
                        errorMessage,
                        "");
                }
                TransactionScope.Complete();
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }

        [SlashCommand("remove-member", "Remove a member from a role")]
        public async Task RemoveMember(
            [
                Summary("bot-role", "The bot role to remove the member from"),
                Autocomplete(typeof(RolesAutocompleteHandler))
            ]
            string roleName,
            [Summary("discord-role", "The role to remove")]
            IRole discordRole
         )
        {
            try
            {
                string errorMessage = null!;
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);

                Core.Role? role = DbContext.Roles.FirstOrDefault(r => r.Name == roleName);
                if (role == null)
                {
                    errorMessage = $"Unknown role {roleName}";
                    Logger.LogWarning("Unknown role { RoleName }", roleName);
                }
                else
                {
                    RoleMember? roleMamber =
                        DbContext.RoleMembers.FirstOrDefault(rm => rm.DiscordGuild == discordGuild
                                                                && rm.Role == role
                                                                && rm.MentionableId == discordRole.Id);
                    if (roleMamber != null)
                    {
                        DbContext.RoleMembers.Remove(roleMamber);
                    }
                    else
                    {
                        errorMessage = $"{MentionUtils.MentionRole(discordRole.Id)} is not a member of the role '{roleName}'.";
                    }
                    DbContext.SaveChanges();
                }

                if (errorMessage == null)
                {
                    await Result.Success(
                        $"Removed {MentionUtils.MentionRole(discordRole.Id)} from role '{roleName}'.", true);
                }
                else
                {
                    await Result.Error(
                        $"Cannot remove {MentionUtils.MentionRole(discordRole.Id)} from role '{roleName}'.",
                        errorMessage,
                        "");
                }
                TransactionScope.Complete();
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }

        [SlashCommand("list", "List role members")]
        public async Task List()
        {
            try
            {
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);

                IList<RoleMember> roleMembers = DbContext.RoleMembers.Include(rm => rm.Role)
                                                         .Where(rm => rm.DiscordGuild == discordGuild)
                                                         .ToList();
                string result = string.Join(
                    Environment.NewLine,
                    roleMembers.OrderBy(rm => rm.Role.Name)
                               .GroupBy(rm => rm.Role)
                               .Select(grp => $"{grp.Key.Name}: {string.Join(", ", grp.ToList().Select(rm => Context.Guild.GetRole(rm.MentionableId).Name).OrderBy(s => s))}"));

                if (!string.IsNullOrEmpty(result))
                {
                    await Result.File(result, $"{Context.Guild.Name} Role Members.txt");
                }
                else
                {
                    await Result.Information("No role members");
                }
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }
    }
}
