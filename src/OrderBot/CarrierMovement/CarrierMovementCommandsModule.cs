using CsvHelper;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Admin;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using System.Globalization;
using System.Text;
using System.Transactions;

namespace OrderBot.CarrierMovement
{
    [Group("carrier-movement", "Monitor carrier movements")]
    [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
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
            /// <param name="auditLogFactory">
            /// </param>
            public Channel(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<Channel> logger,
                DiscordChannelAuditLoggerFactory auditLogFactory)
            {
                ContextFactory = contextFactory;
                Logger = logger;
                AuditLogFactory = auditLogFactory;
            }

            public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
            public ILogger<Channel> Logger { get; }
            public DiscordChannelAuditLoggerFactory AuditLogFactory { get; }

            [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
            // [RequirePerGuildRole("EDAKL Leaders", "EDAKL Veterans")]
            [SlashCommand("set", "Set the channel to receive carrier jump alerts")]
            public async Task Set(
                [Summary("Channel", "Send carrier movement alerts to this channel")]
                IChannel channel
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);
                discordGuild.CarrierMovementChannel = channel.Id;
                await dbContext.SaveChangesAsync();
                auditLogger.Audit($"Set the carrier movement channel to {channel.Name}");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**! Carrier movements will be mentioned in #{channel.Name}. Ensure this bot has 'Send Messages' permission to that channel.",
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
                    message = $"**Success**! Carrier movements will be mentioned in #{channel.Name}";
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
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild);
                if (discordGuild.CarrierMovementChannel != null)
                {
                    discordGuild.CarrierMovementChannel = null;
                    await dbContext.SaveChangesAsync();
                }
                auditLogger.Audit("Cleared carrier alert channel");
                await Context.Interaction.FollowupAsync(
                    text: "**Success**! No alerts sent for carrier movements",
                    ephemeral: true
                );
            }
        }

        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        [Group("ignored-carriers", "Monitor carrier movements")]
        public class IgnoredCarriers : InteractionModuleBase<SocketInteractionContext>
        {
            /// <summary>
            /// Create a new <see cref="IgnoredCarriers"/>.
            /// </summary>
            /// <param name="contextFactory"></param>
            /// <param name="logger"></param>
            public IgnoredCarriers(IDbContextFactory<OrderBotDbContext> contextFactory,
                ILogger<IgnoredCarriers> logger, DiscordChannelAuditLoggerFactory auditLogFactory)
            {
                ContextFactory = contextFactory;
                Logger = logger;
                AuditLogFactory = auditLogFactory;
            }

            public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
            public ILogger<IgnoredCarriers> Logger { get; }
            public DiscordChannelAuditLoggerFactory AuditLogFactory { get; }

            [SlashCommand("add", "Do not track this carrier or report its movements (case insensitive).")]
            public async Task AddIgnoredCarrier(
                [
                     Summary("name", "The full name or just the ending serial number of the carrier to ignore"),
                     Autocomplete(typeof(NotIgnoredCarriersAutocompleteHandler))
                ]
                string name)
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                AddImplementation(dbContext, Context.Guild, new[] { name });
                auditLogger.Audit($"Ignored carrier '{name}'");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**! Fleet carrier '{name}' will **NOT** be tracked and its location reported",
                    ephemeral: true
                );
            }

            internal static void AddImplementation(OrderBotDbContext dbContext, IGuild guild, IEnumerable<string> names)
            {
                using TransactionScope scope = new();
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild,
                    dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers));
                foreach (string name in names)
                {
                    string serialNumber = Carrier.GetSerialNumber(name);
                    Carrier? ignoredCarrier = discordGuild.IgnoredCarriers.FirstOrDefault(c => c.SerialNumber == serialNumber);
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
                }
                scope.Complete();
            }

            [SlashCommand("remove", "Track this carrier and report its movements")]
            public async Task Remove(
                [
                    Summary("Name", "The full name or just the ending serial number of the carrier to track (case insensitive)."),
                    Autocomplete(typeof(IgnoredCarriersAutocompleteHandler))
                ]
                string name
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                RemoveImplementation(dbContext, Context.Guild, name);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                auditLogger.Audit($"Fleet carrier '{name}' removed from ignored list. Its jumps will be reported.");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**! Fleet carrier '{name}' removed from ignored list. Its jumps will be reported.",
                    ephemeral: true
                );
            }

            internal static void RemoveImplementation(OrderBotDbContext dbContext, IGuild guild, string name)
            {
                string serialNumber = Carrier.GetSerialNumber(name);
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild,
                    dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers));
                Carrier? ignoredCarrier = discordGuild.IgnoredCarriers.FirstOrDefault(c => c.SerialNumber == serialNumber);
                if (ignoredCarrier != null)
                {
                    discordGuild.IgnoredCarriers.Remove(ignoredCarrier);
                }
                dbContext.SaveChanges();
            }

            [SlashCommand("list", "List ignored fleet carriers")]
            public async Task List()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                string result = string.Join("\n", ListImplementation(dbContext, Context.Guild).Select(c => c.Name));
                if (!result.Any())
                {
                    await Context.Interaction.FollowupAsync(
                        text: "No ignored fleet carriers",
                        ephemeral: true
                    );
                }
                else
                {
                    using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(result));
                    await Context.Interaction.FollowupWithFileAsync(
                        fileStream: memoryStream,
                        fileName: "IgnoredCarriers.txt",
                        ephemeral: true
                    );
                }
            }

            internal static IEnumerable<Carrier> ListImplementation(OrderBotDbContext dbContext, IGuild guild)
            {
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild,
                    dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers));
                return discordGuild.IgnoredCarriers.OrderBy(c => c.Name);
            }

            [SlashCommand("export", "Export the ignored carriers for backup")]
            public async Task Export()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                IList<CarrierCsvRow> result =
                    ListImplementation(dbContext, Context.Guild)
                        .Select(c => new CarrierCsvRow() { Name = c.Name })
                        .ToList();
                if (result.Count == 0)
                {
                    await Context.Interaction.FollowupAsync(
                        text: "No goals specified",
                        ephemeral: true
                    );
                }
                else
                {
                    using MemoryStream memoryStream = new();
                    using StreamWriter streamWriter = new(memoryStream);
                    using CsvWriter csvWriter = new(streamWriter, CultureInfo.InvariantCulture);
                    csvWriter.WriteRecords(result);
                    csvWriter.Flush();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await Context.Interaction.FollowupWithFileAsync(
                        fileStream: memoryStream,
                        fileName: $"{Context.Guild.Name} Ignored Carriers.csv",
                        ephemeral: true
                    );
                }
            }

            [SlashCommand("import", "Import new goals")]
            public async Task Import(
                [Summary("carriers", "Export output: CSV with ignored carrier name")]
                IAttachment ignoredCarriersAttachement
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                IList<CarrierCsvRow> goals;
                try
                {
                    using (HttpClient client = new())
                    {
                        using Stream stream = await client.GetStreamAsync(ignoredCarriersAttachement.Url);
                        using StreamReader reader = new(stream);
                        using CsvReader csvReader = new(reader, CultureInfo.InvariantCulture);
                        goals = await csvReader.GetRecordsAsync<CarrierCsvRow>().ToListAsync();
                    }

                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                    AddImplementation(dbContext, Context.Guild, goals.Select(g => g.Name));

                    auditLogger.Audit($"Ignored carriers:\n{string.Join("\n", goals.Select(g => g.Name))}");
                    await Context.Interaction.FollowupAsync(
                            text: $"**Success**! {ignoredCarriersAttachement.Filename} added to ignored carriers",
                            ephemeral: true
                    );
                }
                catch (CsvHelperException)
                {
                    await Context.Interaction.FollowupAsync(
                            text: $"**Error**: {ignoredCarriersAttachement.Filename} is not a valid ignored carriers file",
                            ephemeral: true
                        );
                }
                catch (ArgumentException ex)
                {
                    await Context.Interaction.FollowupAsync(
                            text: $"**Error**! {ex.Message}",
                            ephemeral: true
                        );
                }
            }
        }
    }
}
