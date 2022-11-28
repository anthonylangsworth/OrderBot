using CsvHelper;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.Rbac;
using System.Globalization;
using System.Text;
using System.Transactions;

namespace OrderBot.ToDo;

// TODO: Ideally, we would not pass in ToDoListApi and and AuditLogger. However,
// the lack of DI support for SocketInteractionContext curtails this.

[Group("todo-list", "Work supporting minor faction(s)")]
public class ToDoListCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    public ToDoListCommandsModule(OrderBotDbContext dbContext,
        ILogger<ToDoListCommandsModule> logger, ToDoListApiFactory toDoListApiFactory)
    {
        DbContext = dbContext;
        Logger = logger;
        ApiFactory = toDoListApiFactory;
    }

    internal OrderBotDbContext DbContext { get; }
    internal ILogger<ToDoListCommandsModule> Logger { get; }
    internal ToDoListApiFactory ApiFactory { get; }

    [SlashCommand("show", "List the work required for supporting a minor faction")]
    [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
    [RequireBotRole(OfficersRole.RoleName, MembersRole.RoleName, Group = "Permission")]
    public async Task ShowToDoList()
    {
        await ShowTodoListInternal(false);
    }

    [SlashCommand("raw", "List the work required for supporting a minor faction in a copyable format")]
    [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
    [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
    public async Task ShowRawToDoList()
    {
        await ShowTodoListInternal(true);
    }

    private async Task ShowTodoListInternal(bool raw)
    {
        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            ToDoListApi api = ApiFactory.CreateApi(Context.Guild);
            string text = raw
                ? $"```\n{api.GetTodoList()}\n```"
                : api.GetTodoList();
            if (text.Length > DiscordConfig.MaxMessageSize)
            {
                text = text.Substring(0, DiscordConfig.MaxMessageSize);
            }
            await Context.Interaction.FollowupAsync(
                text: text,
                ephemeral: true
            );
        }
        catch (UnknownGoalException ex)
        {
            Logger.LogError(ex, "Unknown goal");
            throw;
        }
        catch (NoSupportedMinorFactionException ex)
        {
            throw new DiscordUserInteractionException(
                "**Error**: No minor faction supported. Support one using `/todo-list support set`.", ex);
        }
    }

    [Group("support", "Support a minor faction")]
    public class Support : InteractionModuleBase<SocketInteractionContext>
    {
        public Support(OrderBotDbContext dbContext, ILogger<Support> logger,
            TextChannelAuditLoggerFactory auditLogFactory, ToDoListApiFactory toDoListApiFactory)
        {
            DbContext = dbContext;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
            ApiFactory = toDoListApiFactory;
        }

        public OrderBotDbContext DbContext { get; }
        public ILogger<Support> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }
        public ToDoListApiFactory ApiFactory { get; }

        [SlashCommand("set", "Set the minor faction this Discord server supports")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Set(
            [Summary("minor-faction", "Start supporting this minor faction")]
            string name
        )
        {
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                await ApiFactory.CreateApi(Context.Guild).SetSupportedMinorFactionAsync(name);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                auditLogger.Audit($"Supporting minor faction *{name}*");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**: Now supporting *{name}*",
                    ephemeral: true
                );
            }
            catch (ArgumentException ex)
            {
                throw new DiscordUserInteractionException($"**Error**: *{name}* is not a known minor faction", ex);
            }
            transactionScope.Complete();
        }

        [SlashCommand("clear", "Stop supporting a minor faction")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Clear()
        {
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            ApiFactory.CreateApi(Context.Guild).ClearSupportedMinorFaction();
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            auditLogger.Audit($"Not supporting any minor faction");
            await Context.Interaction.FollowupAsync(
                text: $"**Success**: Not supporting any minor faction",
                ephemeral: true
            );
            transactionScope.Complete();
        }

        [SlashCommand("get", "Get the minor faction this Discord server supports")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, MembersRole.RoleName, Group = "Permission")]
        public async Task Get()
        {
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            string? minorFactionName = ApiFactory.CreateApi(Context.Guild).GetSupportedMinorFaction()?.Name;
            string message = string.IsNullOrEmpty(minorFactionName)
                ? $"Not supporting any minor faction"
                : $"Supporting *{minorFactionName}*";
            await Context.Interaction.FollowupAsync(
                text: message,
                ephemeral: true
            );
            transactionScope.Complete();
        }
    }

    [Group("goal", "Provide specific intent for a minor faction in a system")]
    public class Goals : InteractionModuleBase<SocketInteractionContext>
    {
        public Goals(OrderBotDbContext dbContext, ILogger<Goals> logger,
            TextChannelAuditLoggerFactory auditLogFactory, ToDoListApiFactory toDoListApiFactory)
        {
            DbContext = dbContext;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
            ApiFactory = toDoListApiFactory;
        }

        public OrderBotDbContext DbContext { get; }
        public ILogger<Goals> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }
        public ToDoListApiFactory ApiFactory { get; }

        [SlashCommand("add", "Set a specific goal for this minor faction in this system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Add(
            [Summary("minor-faction", "The minor faction")]
            string minorFactionName,
            [Summary("star-system", "The star system")]
            string starSystemName,
            [
                Summary("goal", "The intention or aim"),
                Autocomplete(typeof(GoalsAutocompleteHandler))
            ]
            string goalName
        )
        {
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                ApiFactory.CreateApi(Context.Guild).AddGoals(
                    new[] { (minorFactionName, starSystemName, goalName) });
                auditLogger.Audit($"Added goal to {goalName} *{minorFactionName}* in {starSystemName}");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**: Goal {goalName} for *{minorFactionName}* in {starSystemName} added",
                    ephemeral: true
                );
            }
            catch (ArgumentException ex)
            {
                throw new DiscordUserInteractionException($"**Error*: {ex.Message}", ex);
            }
            transactionScope.Complete();
        }

        [SlashCommand("remove", "Remove the specific goal for this minor faction in this system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Remove(
            [
                Summary("minor-faction", "The minor faction"),
                Autocomplete(typeof(KnownMinorFactionsAutocompleteHandler))
            ]
            string minorFactionName,
            [Summary("star-system", "The star system")]
            string starSystemName
        )
        {
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                ApiFactory.CreateApi(Context.Guild).RemoveGoals(minorFactionName, starSystemName);
                auditLogger.Audit($"Removed goal for *{minorFactionName}* in {starSystemName}");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**: Goal for *{minorFactionName}* in {starSystemName} removed",
                    ephemeral: true
                );
            }
            catch (ArgumentException ex)
            {
                throw new DiscordUserInteractionException($"**Error**: {ex.Message}", ex);
            }
            transactionScope.Complete();
        }

        [SlashCommand("list", "List any specific goals per minor faction and per system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, MembersRole.RoleName, Group = "Permission")]
        public async Task List()
        {
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            string result = string.Join(Environment.NewLine,
                ApiFactory.CreateApi(Context.Guild).ListGoals().Select(
                    dgssmfg => $"{dgssmfg.Goal} {dgssmfg.Presence.MinorFaction.Name} in {dgssmfg.Presence.StarSystem.Name}"));
            if (result.Length == 0)
            {
                await Context.Interaction.FollowupAsync(
                        text: "No goals specified",
                        ephemeral: true
                );
            }
            else
            {
                using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(result));
                await Context.Interaction.FollowupWithFileAsync(
                    fileStream: memoryStream,
                    fileName: "Goals.txt",
                    ephemeral: true
                );
            }
            transactionScope.Complete();
        }

        [SlashCommand("export", "Export the current goals for backup")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Export()
        {
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            IList<GoalCsvRow> result = ApiFactory.CreateApi(Context.Guild).ListGoals()
                    .Select(dgssmfg => new GoalCsvRow()
                    {
                        Goal = dgssmfg.Goal,
                        MinorFaction = dgssmfg.Presence.MinorFaction.Name,
                        StarSystem = dgssmfg.Presence.StarSystem.Name
                    })
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
                    fileName: $"{Context.Guild.Name} Goals.csv",
                    ephemeral: true
                );
            }
            transactionScope.Complete();
        }

        [SlashCommand("import", "Import new goals")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Import(
            [Summary("goals", "Export output: CSV with goal, minor faction and star system")]
            IAttachment goalsAttachement
        )
        {
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            IList<GoalCsvRow> goals;
            try
            {
                using (HttpClient client = new())
                {
                    using Stream stream = await client.GetStreamAsync(goalsAttachement.Url);
                    using StreamReader reader = new(stream);
                    using CsvReader csvReader = new(reader, CultureInfo.InvariantCulture);
                    goals = await csvReader.GetRecordsAsync<GoalCsvRow>().ToListAsync();
                }

                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
                ApiFactory.CreateApi(Context.Guild).AddGoals(goals.Select(g => (g.MinorFaction, g.StarSystem, g.Goal)));

                auditLogger.Audit($"Imported goals:\n{string.Join("\n", goals.Select(g => $"{g.Goal} {g.MinorFaction} in {g.StarSystem}"))}");
                await Context.Interaction.FollowupAsync(
                        text: $"**Success**: {goalsAttachement.Filename} added to goals",
                        ephemeral: true
                );
                transactionScope.Complete();
            }
            catch (CsvHelperException)
            {
                await Context.Interaction.FollowupAsync(
                        text: $"**Error**: {goalsAttachement.Filename} is not a valid goals file",
                        ephemeral: true
                    );
            }
            catch (ArgumentException ex)
            {
                throw new DiscordUserInteractionException($"**Error**: {ex.Message}", ex);
            }
        }
    }
}
