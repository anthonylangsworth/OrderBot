using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;

namespace OrderBot.Discord
{
    [Group("carrer-movement", "Monitor carrier movements")]
    public class CarrierMovementCommandsModule : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>
        /// Create a new <see cref="CarrierMovementCommandsModule"/>.
        /// </summary>
        /// <param name="contextFactory"></param>
        /// <param name="logger"></param>
        public CarrierMovementCommandsModule(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<CarrierMovementCommandsModule> logger)
        {
            ContextFactory = contextFactory;
            Logger = logger;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        public ILogger<CarrierMovementCommandsModule> Logger { get; }

        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        // [RequirePerGuildRole("EDAKL Leaders", "EDAKL Veterans")]
        [SlashCommand("set-channel", "Set the channel to receive carrier jump alerts")]
        public async Task SetChannel(
            [Summary("Channel", "Send carrier movement alerts to this channel")]
            IChannel channel
        )
        {
            await Context.Interaction.DeferAsync(ephemeral: true);
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            DiscordGuild? discordGuild = dbContext.DiscordGuilds.FirstOrDefault(dg => dg.GuildId == Context.Guild.Id);
            if (discordGuild == null)
            {
                discordGuild = new DiscordGuild() { GuildId = Context.Guild.Id, CarrierMovementChannel = channel.Id };
            }
            else
            {
                discordGuild.CarrierMovementChannel = channel.Id;
            }
            await dbContext.SaveChangesAsync();
            await Context.Interaction.FollowupAsync(
                text: $"Carrier movements will be mentioned in {channel.Name}",
                ephemeral: true
            );
        }

        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        [SlashCommand("get-channel", "Retrieve the channel that receives carrier jump alerts")]
        public async Task GetChannel()
        {
            await Context.Interaction.DeferAsync(ephemeral: true);
            OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            DiscordGuild? discordGuild = dbContext.DiscordGuilds.FirstOrDefault(dg => dg.GuildId == Context.Guild.Id);
            string message;
            if (discordGuild == null || discordGuild.CarrierMovementChannel == null)
            {
                message = $"No channel set for carrier movements";
            }
            else
            {
                IChannel channel = Context.Guild.GetChannel(discordGuild.CarrierMovementChannel ?? 0);
                message = $"Carrier movements will be mentioned in #{channel.Name}";
            }
            await Context.Interaction.FollowupAsync(
                text: message,
                ephemeral: true
            );
        }

        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        [SlashCommand("mute", "Turn off alerts for carrier jumps")]
        public async Task Mute()
        {
            await Context.Interaction.DeferAsync(ephemeral: true);
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            DiscordGuild? discordGuild = dbContext.DiscordGuilds.FirstOrDefault(dg => dg.GuildId == Context.Guild.Id);
            if (discordGuild != null && discordGuild.CarrierMovementChannel != null)
            {
                discordGuild.CarrierMovementChannel = null;
                await dbContext.SaveChangesAsync();
            }
            await Context.Interaction.FollowupAsync(
                text: "No alerts sent for carrier movements",
                ephemeral: true
            );
        }
    }
}
