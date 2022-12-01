using CsvHelper;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.Rbac;
using System.Globalization;
using System.Text;
using System.Transactions;

namespace OrderBot.CarrierMovement;

[Group("carrier-movement", "Monitor carrier movements")]
public class CarrierMovementCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    [Group("channel", "Send carrier movement alerts")]
    public class Channel : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>
        /// Create a new <see cref="Channel"/>.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        /// <param name="auditLogFactory">
        /// </param>
        public Channel(OrderBotDbContext dbContext,
            ILogger<Channel> logger,
            TextChannelAuditLoggerFactory auditLogFactory)
        {
            DbContext = dbContext;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
        }

        public OrderBotDbContext DbContext { get; }
        public ILogger<Channel> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }

        [SlashCommand("set", "Set the channel to receive carrier jump alerts")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        public async Task Set(
            [Summary("Channel", "Send carrier movement alerts to this channel")]
            IChannel channel
        )
        {
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);
            discordGuild.CarrierMovementChannel = channel.Id;
            await DbContext.SaveChangesAsync();
            auditLogger.Audit($"Set the carrier movement channel to {channel.Name}");
            await Context.Interaction.FollowupAsync(
                text: $"{EphemeralResponse.SuccessPrefix}Carrier movements will be mentioned in {MentionUtils.MentionChannel(channel.Id)}. Ensure this bot has 'Send Messages' permission to that channel. This change takes a few minutes to occur.",
                ephemeral: true
            );
        }

        [SlashCommand("get", "Retrieve the channel that receives carrier jump alerts")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        public async Task Get()
        {
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);
            string message;
            if (discordGuild.CarrierMovementChannel == null)
            {
                message = $"No channel set for carrier movements";
            }
            else
            {
                IChannel channel = Context.Guild.GetChannel(discordGuild.CarrierMovementChannel ?? 0);
                message = $"Carrier movements will be mentioned in {MentionUtils.MentionChannel(channel.Id)}";
            }
            await Context.Interaction.FollowupAsync(
                text: message,
                ephemeral: true
            );
        }

        [SlashCommand("clear", "Turn off alerts for carrier jumps")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        public async Task Clear()
        {
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);
            if (discordGuild.CarrierMovementChannel != null)
            {
                discordGuild.CarrierMovementChannel = null;
                await DbContext.SaveChangesAsync();
            }
            auditLogger.Audit("Cleared carrier alert channel");
            await Context.Interaction.FollowupAsync(
                text: "{MessagePrefix.Success}No alerts sent for carrier movements",
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
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        public IgnoredCarriers(OrderBotDbContext dbContext,
            ILogger<IgnoredCarriers> logger,
            TextChannelAuditLoggerFactory auditLogFactory)
        {
            DbContext = dbContext;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
        }

        public OrderBotDbContext DbContext { get; }
        public ILogger<IgnoredCarriers> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }

        [SlashCommand("add", "Do not track this carrier or report its movements (case insensitive).")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Add(
            [
                 Summary("name", "The full name or just the ending serial number of the carrier to ignore"),
                 Autocomplete(typeof(NotIgnoredCarriersAutocompleteHandler))
            ]
            string name)
        {
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            try
            {
                AddImplementation(DbContext, Context.Guild, new[] { name });
                auditLogger.Audit($"Ignored carrier '{name}'");
                await Context.Interaction.FollowupAsync(
                    text: $"{EphemeralResponse.SuccessPrefix}Fleet carrier '{name}' will be ignored and its jumps **NOT** reported",
                    ephemeral: true
                );
            }
            catch (ArgumentException ex)
            {
                throw new DiscordUserInteractionException(ex.Message, ex);
            }
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
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Remove(
            [
                Summary("Name", "The full name or just the ending serial number of the carrier to track (case insensitive)."),
                Autocomplete(typeof(IgnoredCarriersAutocompleteHandler))
            ]
            string name
        )
        {
            RemoveImplementation(DbContext, Context.Guild, name);
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            auditLogger.Audit($"Fleet carrier '{name}' removed from ignored list. Its jumps will be reported.");
            await Context.Interaction.FollowupAsync(
                text: $"{EphemeralResponse.SuccessPrefix}Fleet carrier '{name}' removed from ignored list. Its jumps will be reported.",
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
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, MembersRole.RoleName, Group = "Permission")]
        public async Task List()
        {
            string result = string.Join("\n", ListImplementation(DbContext, Context.Guild).Select(c => c.Name));
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
                    fileName: $"{Context.Guild.Name} Ignored Carriers.txt",
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
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Export()
        {
            IList<CarrierCsvRow> result =
                ListImplementation(DbContext, Context.Guild)
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
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Import(
            [Summary("carriers", "Export output: CSV with ignored carrier name")]
            IAttachment ignoredCarriersAttachement
        )
        {
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

                AddImplementation(DbContext, Context.Guild, goals.Select(g => g.Name));

                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                auditLogger.Audit($"Ignored carriers:\n{string.Join("\n", goals.Select(g => g.Name))}");
                await Context.Interaction.FollowupAsync(
                        text: $"{EphemeralResponse.SuccessPrefix}{ignoredCarriersAttachement.Filename} added to ignored carriers",
                        ephemeral: true
                );
            }
            catch (CsvHelperException ex)
            {
                throw new DiscordUserInteractionException(
                    $"{ignoredCarriersAttachement.Filename} is not a valid ignored carriers file", ex);
            }
            catch (ArgumentException ex)
            {
                throw new DiscordUserInteractionException(ex.Message, ex);
            }
        }
    }
}
