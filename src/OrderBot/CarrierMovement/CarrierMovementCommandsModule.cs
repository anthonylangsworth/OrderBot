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
using System.Transactions;

namespace OrderBot.CarrierMovement;

[Group("carrier-movement", "Monitor carrier movements")]
public class CarrierMovementCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    [Group("channel", "Send carrier movement alerts")]
    public class Channel : BaseCommandsModule<Channel>
    {
        /// <summary>
        /// Create a new <see cref="Channel"/>.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        /// <param name="auditLogFactory">
        /// </param>
        /// <param name="resultFactory">
        /// </param>
        public Channel(OrderBotDbContext dbContext,
            ILogger<Channel> logger,
            TextChannelAuditLoggerFactory auditLogFactory,
            ResultFactory resultFactory)
            : base(dbContext, logger, auditLogFactory, resultFactory)
        {
            // Do nothing
        }

        [SlashCommand("set", "Set the channel to receive carrier jump alerts")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        public async Task Set(
            [Summary("Channel", "Send carrier movement alerts to this channel")]
            IChannel channel
        )
        {
            try
            {
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);
                discordGuild.CarrierMovementChannel = channel.Id;
                await DbContext.SaveChangesAsync();
                await Result.Success(
                    $"Carrier movements will be mentioned in {MentionUtils.MentionChannel(channel.Id)}. Ensure this bot has 'Send Messages' permission to that channel. This change takes a few minutes to occur.",
                    true);
                TransactionScope.Complete();
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }

        [SlashCommand("get", "Retrieve the channel that receives carrier jump alerts")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        public async Task Get()
        {
            try
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
                await Result.Information(message);
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }

        [SlashCommand("clear", "Turn off alerts for carrier jumps")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        public async Task Clear()
        {
            try
            {
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Context.Guild);
                if (discordGuild.CarrierMovementChannel != null)
                {
                    discordGuild.CarrierMovementChannel = null;
                    await DbContext.SaveChangesAsync();
                }
                await Result.Success(
                    "No alerts sent for carrier movements",
                    true
                );
                TransactionScope.Complete();
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }
    }

    [Group("ignored-carriers", "Monitor carrier movements")]
    public class IgnoredCarriers : BaseCommandsModule<IgnoredCarriers>
    {
        /// <summary>
        /// Create a new <see cref="IgnoredCarriers"/>.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        /// <param name="auditLogFactory"></param>
        /// <param name="resultFactory"></param>
        public IgnoredCarriers(OrderBotDbContext dbContext,
            ILogger<IgnoredCarriers> logger,
            TextChannelAuditLoggerFactory auditLogFactory,
            ResultFactory resultFactory)
            : base(dbContext, logger, auditLogFactory, resultFactory)
        {
            // Do nothing
        }

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
            try
            {
                AddImplementation(DbContext, Context.Guild, new[] { name });
                await Result.Success(
                    $"Fleet carrier '{name}' will be ignored and its jumps **NOT** reported", true);
                TransactionScope.Complete();
            }
            catch (CarrierNameException ex)
            {
                await Result.Error(
                    $"Cannot ignore the carrier '{ex.CarrierName}'.",
                    $"'{ex.CarrierName}' lacks or has an invalid serial number suffix in the form of XXX-XXX.",
                    "Correct the carrier name and try again.");
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }

        internal static void AddImplementation(OrderBotDbContext dbContext, IGuild guild, IEnumerable<string> names)
        {
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
            try
            {
                RemoveImplementation(DbContext, Context.Guild, name);
                await Result.Success(
                    $"Fleet carrier '{name}' removed from ignored list. Its jumps will be reported.", true);
                TransactionScope.Complete();
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
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
            try
            {
                string result = string.Join("\n", ListImplementation(DbContext, Context.Guild).Select(c => c.Name));
                if (!result.Any())
                {
                    await Result.Information("No ignored fleet carriers");
                }
                else
                {
                    await Result.File(result, $"{Context.Guild.Name} Ignored Carriers.txt");
                }
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
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
            try
            {
                IList<CarrierCsvRow> result =
                    ListImplementation(DbContext, Context.Guild)
                        .Select(c => new CarrierCsvRow() { Name = c.Name })
                        .ToList();
                if (result.Count == 0)
                {
                    await Result.Information("No goals specified");
                }
                else
                {
                    await Result.CsvFile(result, $"{Context.Guild.Name} Ignored Carriers.csv");
                }
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
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
            try
            {
                IList<CarrierCsvRow> goals;
                using (HttpClient client = new())
                {
                    using Stream stream = await client.GetStreamAsync(ignoredCarriersAttachement.Url);
                    using StreamReader reader = new(stream);
                    using CsvReader csvReader = new(reader, CultureInfo.InvariantCulture);
                    goals = await csvReader.GetRecordsAsync<CarrierCsvRow>().ToListAsync();
                }

                AddImplementation(DbContext, Context.Guild, goals.Select(g => g.Name));

                await Result.Information($"{ignoredCarriersAttachement.Filename} added to ignored carriers");

                TransactionScope.Complete();
            }
            catch (CsvHelperException)
            {
                await Result.Error(
                    "Cannot import ignored carriers from the file.",
                    $"{ignoredCarriersAttachement.Filename} is not a valid ignored carriers file.",
                    "Correct the file then import it again.");
            }
            catch (CarrierNameException ex)
            {
                await Result.Error(
                    "Cannot import ignored carriers from the file.",
                    $"{ex.CarrierName} is not a valid carrier name.",
                    "Correct the file then import it again.");
            }
            catch (Exception ex)
            {
                await Result.Exception(ex);
            }
        }
    }
}
