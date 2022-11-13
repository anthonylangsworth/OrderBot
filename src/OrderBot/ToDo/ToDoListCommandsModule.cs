using CsvHelper;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Audit;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.Rbac;
using System.Globalization;
using System.Text;

namespace OrderBot.ToDo;

[Group("todo-list", "Work supporting minor faction(s)")]
public class ToDoListCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    public ToDoListCommandsModule(IDbContextFactory<OrderBotDbContext> contextFactory,
        ILogger<Support> logger, ToDoListApi toDoListApi)
    {
        ContextFactory = contextFactory;
        Logger = logger;
        Api = toDoListApi;
    }

    public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
    public ILogger<Support> Logger { get; }
    public ToDoListApi Api { get; }

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
        using IDisposable loggerScope = Logger.BeginScope(new ScopeBuilder(Context).Build());
        using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
        try
        {
            await Context.Interaction.FollowupAsync(
                text: raw
                    ? $"```\n{Api.GetTodoList(dbContext, Context.Guild)}\n```"
                    : Api.GetTodoList(dbContext, Context.Guild),
                ephemeral: true
            );
        }
        catch (UnknownGoalException ex)
        {
            Logger.LogError(ex, "Unknown goal");
            throw;
        }
        catch (NoSupportedMinorFactionException)
        {
            await Context.Interaction.FollowupAsync(
                text: "**Error** No minor faction supported. Support one using `/todo-list support set`.",
                ephemeral: true
            );
        }
    }

    [Group("support", "Support a minor faction")]
    public class Support : InteractionModuleBase<SocketInteractionContext>
    {
        public Support(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<Support> logger,
            TextChannelAuditLoggerFactory auditLogFactory, ToDoListApi api)
        {
            ContextFactory = contextFactory;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
            Api = api;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        public ILogger<Support> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }
        public ToDoListApi Api { get; }

        [SlashCommand("set", "Set the minor faction this Discord server supports")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Set(
            [Summary("minor-faction", "Start supporting this minor faction")]
            string name
        )
        {
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            string message;
            try
            {
                Api.SetSupportedMinorFaction(dbContext, Context.Guild, name);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                auditLogger.Audit($"Supporting minor faction *{name}*");
                message = $"**Success**! Now supporting *{name}*";
            }
            catch (ArgumentException)
            {
                message = $"**Error**: *{name}* is not a known minor faction";
            }
            await Context.Interaction.FollowupAsync(
                text: message,
                ephemeral: true
            );
        }

        [SlashCommand("clear", "Stop supporting a minor faction")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Clear()
        {
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            Api.ClearSupportedMinorFaction(dbContext, Context.Guild);
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            auditLogger.Audit($"Not supporting any minor faction");
            await Context.Interaction.FollowupAsync(
                text: $"**Success**! Not supporting any minor faction",
                ephemeral: true
            );
        }

        [SlashCommand("get", "Get the minor faction this Discord server supports")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, MembersRole.RoleName, Group = "Permission")]
        public async Task Get()
        {
            using OrderBotDbContext dbContext = await ContextFactory.CreateDbContextAsync();
            string? minorFactionName = Api.GetSupportedMinorFaction(dbContext, Context.Guild)?.Name;
            string message = string.IsNullOrEmpty(minorFactionName)
                ? $"Not supporting any minor faction"
                : $"Supporting *{minorFactionName}*";
            await Context.Interaction.FollowupAsync(
                text: message,
                ephemeral: true
            );
        }
    }

    [Group("goal", "Provide specific intent for a minor faction in a system")]
    public class Goals : InteractionModuleBase<SocketInteractionContext>
    {
        public Goals(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<Goals> logger,
            TextChannelAuditLoggerFactory auditLogFactory, ToDoListApi api)
        {
            ContextFactory = contextFactory;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
            Api = api;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        public ILogger<Goals> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }
        public ToDoListApi Api { get; }

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
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            try
            {
                Api.AddGoals(dbContext, Context.Guild,
                    new[] { (minorFactionName, starSystemName, goalName) });
                auditLogger.Audit($"Added goal to {goalName} *{minorFactionName}* in {starSystemName}");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**! Goal {goalName} for *{minorFactionName}* in {starSystemName} added",
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

        [SlashCommand("remove", "Remove the specific goal for this minor faction in this system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Remove(
            [Summary("minor-faction", "The minor faction")]
            string minorFactionName,
            [Summary("star-system", "The star system")]
            string starSystemName
        )
        {
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            try
            {
                Api.RemoveGoals(dbContext, Context.Guild, minorFactionName, starSystemName);
                auditLogger.Audit($"Removed goal for *{minorFactionName}* in {starSystemName}");
                await Context.Interaction.FollowupAsync(
                    text: $"**Success**! Goal for *{minorFactionName}* in {starSystemName} removed",
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

        [SlashCommand("list", "List any specific goals per minor faction and per system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, MembersRole.RoleName, Group = "Permission")]
        public async Task List()
        {
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            string result = string.Join(Environment.NewLine,
                Api.ListGoals(dbContext, Context.Guild).Select(
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
        }

        [SlashCommand("export", "Export the current goals for backup")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Export()
        {
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            IList<GoalCsvRow> result = Api.ListGoals(dbContext, Context.Guild)
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

                using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                Api.AddGoals(dbContext, Context.Guild,
                    goals.Select(g => (g.MinorFaction, g.StarSystem, g.Goal)));

                auditLogger.Audit($"Imported goals:\n{string.Join("\n", goals.Select(g => $"{g.Goal} {g.MinorFaction} in {g.StarSystem}"))}");
                await Context.Interaction.FollowupAsync(
                        text: $"**Success**! {goalsAttachement.Filename} added to goals",
                        ephemeral: true
                );
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
                await Context.Interaction.FollowupAsync(
                        text: $"**Error**! {ex.Message}",
                        ephemeral: true
                    );
            }
        }
    }
}
