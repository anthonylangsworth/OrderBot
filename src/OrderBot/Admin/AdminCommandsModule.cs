using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Admin;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;

namespace OrderBot.CarrierMovement
{
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
            /// <param name="contextFactory"></param>
            /// <param name="logger"></param>
            public AuditChannel(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<AuditChannel> logger,
                DiscordChannelAuditLogFactory auditLogFactory)
            {
                ContextFactory = contextFactory;
                Logger = logger;
                AuditLogFactory = auditLogFactory;
            }

            public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
            public ILogger<AuditChannel> Logger { get; }
            public DiscordChannelAuditLogFactory AuditLogFactory { get; }

            [SlashCommand("set", "Change the channel to which audit messages are written")]
            public async Task Set(
                [Summary("Channel", "Write audit messages to this chanel")]
                IChannel channel
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using (Logger.BeginScope(("bgs-order-bot audit-channel set", Context.Guild.Name, channel.Name)))
                {
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
                            throw new ArgumentException($"Cannot write to new audit channel {newAuditChannel?.Mention}. Ensure the bot has 'Send Messages' permission.", ex);
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"{MentionUtils.MentionChannel(channel.Id)} must be a text channel");
                    }

                    discordGuild.AuditChannel = channel.Id;
                    await dbContext.SaveChangesAsync();
                    await Context.Interaction.FollowupAsync(
                        text: $"Audit messages will be written to {MentionUtils.MentionChannel(channel.Id)}.",
                        ephemeral: true
                    );
                }
            }

            [SlashCommand("get", "Get the channel to which audit log messages are written")]
            public async Task Get()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using (Logger.BeginScope(("bgs-order-bot audit-channel  get", Context.Guild.Name)))
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
            }

            [SlashCommand("clear", "Turn off auditing")]
            public async Task Clear()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using (Logger.BeginScope(("bgs-order-bot audit-channel clear", Context.Guild.Name)))
                {
                    using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
                    DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);
                    AuditLogFactory.CreateAuditLog(Context).Audit(discordGuild, "Auditing disabled");
                    if (discordGuild.AuditChannel != null)
                    {
                        discordGuild.AuditChannel = null;
                        await dbContext.SaveChangesAsync();
                    }
                    await Context.Interaction.FollowupAsync(
                        text: "Auditing is turned off",
                        ephemeral: true
                    );
                }
            }
        }
    }
}
