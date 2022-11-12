using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Audit;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.Rbac;
using System.Text;

namespace OrderBot.Admin;

[Group("bgs-order-bot", "BGS Order Bot Admin commands")]
[RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
public class AdminCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    [Group("audit-channel", "Audit events")]
    public class AuditChannel : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>
        /// Create a new <see cref="Audit"/>.
        /// </summary>
        /// <param name="contextFactory">
        /// </param>
        /// <param name="logger">
        /// A <see cref="Logger{AuditChannel}"/> to log to.
        /// </param>
        /// <param name="auditLogFactory">
        /// </param>
        public AuditChannel(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<AuditChannel> logger,
            TextChannelAuditLoggerFactory auditLogFactory)
        {
            ContextFactory = contextFactory;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        public ILogger<AuditChannel> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }

        [SlashCommand("set", "Change the channel to which audit messages are written")]
        public async Task Set(
            [Summary("Channel", "Write audit messages to this chanel")]
            IChannel channel
        )
        {
            string errorMessage = null!;
            using IDisposable loggerScope = Logger.BeginScope(new ScopeBuilder(Context).Build());
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);

            if (Context.Guild.GetChannel(channel.Id) is SocketTextChannel newAuditChannel)
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
                    Logger.LogError(ex, "Cannot write to old audit channel {channelName}. Ensure the bot has 'Send Messages' permission.",
                        oldAuditChannel?.Name ?? oldAuditChannel?.Id.ToString());
                }

                try
                {
#pragma warning disable CS8602
                    await newAuditChannel.SendMessageAsync(auditMessage);
#pragma warning restore CS8602
                }
                catch (Exception ex)
                {
                    errorMessage = $"**Error**: Cannot write to new audit channel {newAuditChannel?.Mention}. Ensure the bot has 'Send Messages' permission.";
                    Logger.LogWarning(ex, "Cannot write to audit channel {ChannelId}.", newAuditChannel?.Id ?? 0);
                }
            }
            else
            {
                errorMessage = $"**Error**: {MentionUtils.MentionChannel(channel.Id)} must be a text channel";
                Logger.LogWarning("{ChannelId} is not a text channel", channel?.Id);
            }

            if (errorMessage != null)
            {
                await Context.Interaction.FollowupAsync(
                    text: errorMessage,
                    ephemeral: true
                );
            }
            else
            {
                discordGuild.AuditChannel = channel?.Id ?? 0;
                await dbContext.SaveChangesAsync();
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**: Audit messages will now be written to {MentionUtils.MentionChannel(channel?.Id ?? 0)}.",
                    ephemeral: true
                );
            }
        }

        [SlashCommand("get", "Get the channel to which audit log messages are written")]
        public async Task Get()
        {
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);
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
            await Context.Interaction.FollowupAsync(
                text: message,
                ephemeral: true
            );
        }

        [SlashCommand("clear", "Turn off auditing")]
        public async Task Clear()
        {
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);
            if (discordGuild.AuditChannel != null)
            {
                discordGuild.AuditChannel = null;
                await dbContext.SaveChangesAsync();
            }
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            auditLogger.Audit("Auditing disabled");
            await Context.Interaction.FollowupAsync(
                text: "Auditing is turned off",
                ephemeral: true
            );
        }
    }

    [Group("rbac", "Role-based Access Control")]
    public class Rbac : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>
        /// Create a new <see cref="Audit"/>.
        /// </summary>
        /// <param name="contextFactory">
        /// </param>
        /// <param name="logger">
        /// A <see cref="Logger{AuditChannel}"/> to log to.
        /// </param>
        /// <param name="auditLogFactory">
        /// </param>
        public Rbac(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<AuditChannel> logger,
            TextChannelAuditLoggerFactory auditLogFactory)
        {
            ContextFactory = contextFactory;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        public ILogger<AuditChannel> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }

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
            string errorMessage = null!;
            using IDisposable loggerScope = Logger.BeginScope(new ScopeBuilder(Context).Build());
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);

            if (!Roles.Map.Keys.Contains(roleName))
            {
                errorMessage = $"Unknown role {roleName}";
                Logger.LogWarning("Unknown role { RoleName }", roleName);
            }
            else
            {
                Core.Role? role = dbContext.Roles.FirstOrDefault(r => r.Name == roleName);
                if (role == null)
                {
                    role = new Core.Role { Name = roleName };
                    dbContext.Roles.Add(role);
                }

                RoleMember? roleMamber =
                    dbContext.RoleMembers.FirstOrDefault(rm => rm.DiscordGuild == discordGuild
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
                    dbContext.RoleMembers.Add(roleMamber);
                }

                dbContext.SaveChanges();
            }

            if (errorMessage == null)
            {
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                auditLogger.Audit($"Added '{discordRole.Name}' to role '{roleName}'");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**: Added '{discordRole.Name}' to role '{roleName}'",
                    ephemeral: true
                );
            }
            else
            {
                await Context.Interaction.FollowupAsync(
                    text: $"**Error**: {errorMessage}",
                    ephemeral: true
                );
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
            string errorMessage = null!;
            using IDisposable loggerScope = Logger.BeginScope(new ScopeBuilder(Context).Build());
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);

            Core.Role? role = dbContext.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                errorMessage = $"Unknown role {roleName}";
                Logger.LogWarning("Unknown role { RoleName }", roleName);
            }
            else
            {
                RoleMember? roleMamber =
                    dbContext.RoleMembers.FirstOrDefault(rm => rm.DiscordGuild == discordGuild
                                                            && rm.Role == role
                                                            && rm.MentionableId == discordRole.Id);
                if (roleMamber != null)
                {
                    dbContext.RoleMembers.Remove(roleMamber);
                }
                dbContext.SaveChanges();
            }

            if (errorMessage == null)
            {
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                auditLogger.Audit($"Removed '{discordRole.Name}' from role '{roleName}'");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**: Removed '{discordRole.Name}' from role '{roleName}'",
                    ephemeral: true
                );
            }
            else
            {
                await Context.Interaction.FollowupAsync(
                    text: $"**Error**: {errorMessage}",
                    ephemeral: true
                );
            }
        }

        [SlashCommand("list", "List role members")]
        public async Task List()
        {
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);

            IList<RoleMember> roleMembers = dbContext.RoleMembers.Include(rm => rm.Role)
                                                     .Where(rm => rm.DiscordGuild == discordGuild)
                                                     .ToList();
            string result = string.Join(
                Environment.NewLine,
                roleMembers.OrderBy(rm => rm.Role.Name)
                           .GroupBy(rm => rm.Role)
                           .Select(grp => $"{grp.Key.Name}: {string.Join(", ", grp.ToList().Select(rm => Context.Guild.GetRole(rm.MentionableId).Name).OrderBy(s => s))}"));

            if (!string.IsNullOrEmpty(result))
            {
                using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(result));
                await Context.Interaction.FollowupWithFileAsync(
                    fileStream: memoryStream,
                    fileName: $"{Context.Guild.Name} Role Members.txt",
                    ephemeral: true
                );
            }
            else
            {
                await Context.Interaction.FollowupAsync(
                    text: $"No role members",
                    ephemeral: true
                );
            }
        }
    }
}
