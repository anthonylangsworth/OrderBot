using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using OrderBot.Discord;

namespace OrderBot.CarrierMovement
{
    [Group("carrier-movement", "Monitor carrier movements")]
    public class CarrierMovementCommandsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("channel", "Send carrier movement alerts")]
        public class Channel : InteractionModuleBase<SocketInteractionContext>
        {
            /// <summary>
            /// Create a new <see cref="IgnoredCarriers"/>.
            /// </summary>
            /// <param name="contextFactory"></param>
            /// <param name="logger"></param>
            public Channel(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<Channel> logger)
            {
                ContextFactory = contextFactory;
                Logger = logger;
            }

            public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
            public ILogger<Channel> Logger { get; }

            [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
            // [RequirePerGuildRole("EDAKL Leaders", "EDAKL Veterans")]
            [SlashCommand("set", "Set the channel to receive carrier jump alerts")]
            public async Task Set(
                [Summary("Channel", "Send carrier movement alerts to this channel")]
                IChannel channel
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);
                discordGuild.CarrierMovementChannel = channel.Id;
                await dbContext.SaveChangesAsync();
                await Context.Interaction.FollowupAsync(
                    text: $"Carrier movements will be mentioned in #{channel.Name}. Ensure this bot has 'Send Messages' permission to that channel.",
                    ephemeral: true
                );
            }

            [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
            [SlashCommand("get", "Retrieve the channel that receives carrier jump alerts")]
            public async Task Get()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);
                string message;
                if (discordGuild.CarrierMovementChannel == null)
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
            [SlashCommand("clear", "Turn off alerts for carrier jumps")]
            public async Task Clear()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);
                if (discordGuild.CarrierMovementChannel != null)
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

        [Group("ignored-carriers", "Monitor carrier movements")]
        public class IgnoredCarriers : InteractionModuleBase<SocketInteractionContext>
        {
            /// <summary>
            /// Create a new <see cref="IgnoredCarriers"/>.
            /// </summary>
            /// <param name="contextFactory"></param>
            /// <param name="logger"></param>
            public IgnoredCarriers(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<IgnoredCarriers> logger)
            {
                ContextFactory = contextFactory;
                Logger = logger;
            }

            public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
            public ILogger<IgnoredCarriers> Logger { get; }

            [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
            [SlashCommand("add", "Do not track this carrier or report its movements (case insensitive).")]
            public async Task AddIgnoredCarrier(
                [
                     Summary("name", "The full name or just the ending serial number of the carrier to ignore"),
                     Autocomplete(typeof(TrackedCarriersAutocompleteHandler))
                ]
                string name)
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                if (!Carrier.IsCarrier(name))
                {
                    await Context.Interaction.FollowupAsync(
                        text: $"'{name}' is not a valid carrier name",
                        ephemeral: true
                    );
                }
                else
                {
                    string serialNumber = Carrier.GetSerialNumber(name);
                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                    DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild,
                        dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers));
                    if (!discordGuild.IgnoredCarriers.Any(c => c.SerialNumber == serialNumber))
                    {
                        Carrier? carrier = dbContext.Carriers.FirstOrDefault(c => c.SerialNumber == serialNumber);
                        if (carrier == null)
                        {
                            carrier = new Carrier() { Name = name };
                            dbContext.Carriers.Add(carrier);
                        }
                        discordGuild.IgnoredCarriers.Add(carrier);
                    }
                    dbContext.SaveChanges();
                    await Context.Interaction.FollowupAsync(
                        text: $"Fleet carrier '{name}' will **NOT** be tracked or its location reported",
                        ephemeral: true
                    );
                }
            }

            [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
            [SlashCommand("remove", "Track this carrier and report its movements")]
            public async Task RemoveIgnoredCarrier(
                [Summary("Name", "The full name or just the ending serial number of the carrier to track (case insensitive).")]
                string name
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                if (!Carrier.IsCarrier(name))
                {
                    await Context.Interaction.FollowupAsync(
                        text: $"'{name}' is not a valid carrier name",
                        ephemeral: true
                    );
                }
                else
                {
                    string serialNumber = Carrier.GetSerialNumber(name);
                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                    DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild,
                        dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers));
                    Carrier? ignoredCarrier = discordGuild.IgnoredCarriers.FirstOrDefault(c => c.SerialNumber == serialNumber);
                    if (ignoredCarrier != null)
                    {
                        discordGuild.IgnoredCarriers.Remove(ignoredCarrier);
                    }
                    dbContext.SaveChanges();
                    await Context.Interaction.FollowupAsync(
                        text: $"Fleet carrier '{name}' will be tracked and its location reported",
                        ephemeral: true
                    );
                }
            }

            [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
            [SlashCommand("list", "List ignored fleet carriers")]
            public async Task ListIgnoredCarriers()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild,
                    dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers));
                string result = string.Join("\n", discordGuild.IgnoredCarriers.OrderBy(c => c.Name)
                                                                              .Select(c => c.Name));
                if (!result.Any())
                {
                    result = "No ignored fleet carriers";
                }
                await Context.Interaction.FollowupAsync(
                    text: result,
                    ephemeral: true
                );
            }
        }

        //// TODO: Use constants
        //[AutocompleteCommand("ignore-carrier", "name")]
        //public async Task TrackedCarriersAutocomplete()
        //{
        //    // See https://discordnet.dev/guides/int_framework/autocompletion.html

        //    if (Context.Interaction != null && Context.Interaction is SocketAutocompleteInteraction interaction)
        //    {
        //        string currentValue = interaction.Data.Current.Value.ToString() ?? "";
        //        using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
        //        DiscordGuild? discordGuild = dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
        //                                                            .FirstOrDefault(dg => dg.GuildId == Context.Guild.Id);
        //        IList<AutocompleteResult> carrierNames;
        //        carrierNames = (discordGuild != null ? dbContext.Carriers.Except(discordGuild.IgnoredCarriers) : dbContext.Carriers)
        //                            .Where(c => c.Name.StartsWith(currentValue))
        //                            .OrderBy(c => c.Name)
        //                            .Select(c => new AutocompleteResult(c.Name, c.Name))
        //                            .ToList();

        //        // max - 25 suggestions at a time (API limit)
        //        await interaction.RespondAsync(carrierNames.Take(25));
        //    }
        //    else
        //    {
        //        Logger.LogWarning("Invalid interaction");
        //    }
        //}
    }
}
