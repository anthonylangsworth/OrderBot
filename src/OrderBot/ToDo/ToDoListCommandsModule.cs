using CsvHelper;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.Rbac;
using System.Globalization;
using System.Transactions;

namespace OrderBot.ToDo;

// TODO: Ideally, we would not pass in ToDoListApi and and AuditLogger. However,
// the lack of DI support for SocketInteractionContext curtails this.

[Group("todo-list", "Work supporting minor faction(s)")]
public class ToDoListCommandsModule : BaseTodoListCommandsModule<ToDoListCommandsModule>
{
    public ToDoListCommandsModule(OrderBotDbContext dbContext,
        ILogger<ToDoListCommandsModule> logger,
        ToDoListApiFactory toDoListApiFactory,
        TextChannelAuditLoggerFactory auditLogFactory,
        ResponseFactory responseFactory)
            : base(dbContext, logger, toDoListApiFactory, auditLogFactory, responseFactory)
    {
        // DO nothing
    }

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
        try
        {
            string text = raw
                ? $"```\n{Api.GetTodoList()}\n```"
                : Api.GetTodoList();
            await Response.Information(text);
        }
        catch (UnknownGoalException ex)
        {
            Logger.LogError(ex, "Unknown goal");
            await Response.Error(
                "Cannot generate suggestions.",
                $"There is an unknown goal '{ex.Goal}' for *{ex.MinorFaction}* in {ex.StarSystem}.",
                "Remove that goal using `/todo-list goal remove` then re-run this command.");
        }
        catch (NoSupportedMinorFactionException)
        {
            await Response.Error(
                "Cannot generate suggestions.",
                "There is no supported minor faction.",
                "Support a minor faction using `/todo-list support set` then re-run this command.");
        }
        catch (Exception ex)
        {
            await Response.Exception(ex);
        }
    }

    [Group("support", "Support a minor faction")]
    public class Support : BaseTodoListCommandsModule<Support>
    {
        public Support(OrderBotDbContext dbContext,
            ILogger<Support> logger,
            ToDoListApiFactory toDoListApiFactory,
            TextChannelAuditLoggerFactory auditLogFactory,
            ResponseFactory responseFactory)
            : base(dbContext, logger, toDoListApiFactory, auditLogFactory, responseFactory)
        {
            // Do nothing
        }

        [SlashCommand("set", "Set the minor faction this Discord server supports")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Set(
            [
                Summary("minor-faction", "Start supporting this minor faction"),
                Autocomplete(typeof(MinorFactionsAutocompleteHandler))
            ]
            string name
        )
        {
            try
            {
                await Api.SetSupportedMinorFactionAsync(name);
                await Response.Success($"Now supporting minor faction *{name}*", true);
                TransactionScope.Complete();
            }
            catch (UnknownMinorFactionException)
            {
                await Response.Error(
                    "Cannot support that minor faction.",
                    $"The minor faction *{name}* not exist.",
                    "Try again, checking the spelling and capitalization carefully.");
            }
            catch (Exception ex)
            {
                await Response.Exception(ex);
            }
        }

        [SlashCommand("clear", "Stop supporting a minor faction")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Clear()
        {
            try
            {
                Api.ClearSupportedMinorFaction();
                await Response.Success("Not supporting any minor faction", true);
                TransactionScope.Complete();
            }
            catch (Exception ex)
            {
                await Response.Exception(ex);
            }
        }

        [SlashCommand("get", "Get the minor faction this Discord server supports")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, MembersRole.RoleName, Group = "Permission")]
        public async Task Get()
        {
            try
            {
                string? minorFactionName = Api.GetSupportedMinorFaction()?.Name;
                string message = string.IsNullOrEmpty(minorFactionName)
                    ? $"Not supporting any minor faction"
                    : $"Supporting *{minorFactionName}*";
                await Response.Information(message);
            }
            catch (Exception ex)
            {
                await Response.Exception(ex);
            }
        }
    }

    [Group("goal", "Provide specific intent for a minor faction in a system")]
    public class Goals : BaseTodoListCommandsModule<Goals>
    {
        public Goals(OrderBotDbContext dbContext,
            ILogger<Goals> logger,
            ToDoListApiFactory toDoListApiFactory,
            TextChannelAuditLoggerFactory auditLogFactory,
            ResponseFactory responseFactory)
            : base(dbContext, logger, toDoListApiFactory, auditLogFactory, responseFactory)
        {
            // Do nothing
        }

        [SlashCommand("add", "Set a specific goal for this minor faction in this system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Add(
            [
                Summary("minor-faction", "The minor faction"),
                Autocomplete(typeof(MinorFactionsAutocompleteHandler))
            ]
            string minorFactionName,
            [
                Summary("star-system", "The star system"),
                Autocomplete(typeof(StarSystemsAutocompleteHandler))
            ]
            string starSystemName,
            [
                Summary("goal", "The intention or aim"),
                Autocomplete(typeof(GoalsAutocompleteHandler))
            ]
            string goalName
        )
        {
            try
            {
                await Api.AddGoals(
                    new[] { (minorFactionName, starSystemName, goalName) });
                await Response.Success($"Added goal to {goalName} *{minorFactionName}* in {starSystemName}", true);
                TransactionScope.Complete();
            }
            catch (UnknownMinorFactionException)
            {
                await Response.Error(
                    $"Cannot add the goal to {goalName} *{minorFactionName}* in {starSystemName}.",
                    $"The minor faction *{minorFactionName}* does not exist.",
                    "Try again, checking the spelling and capitalization carefully.");
            }
            catch (UnknownStarSystemException)
            {
                await Response.Error(
                    $"Cannot add the goal to {goalName} *{minorFactionName}* in {starSystemName}.",
                    $"The star system *{starSystemName}* does not exist.",
                    "Try again, checking the spelling and capitalization carefully.");
            }
            catch (UnknownGoalException)
            {
                await Response.Error(
                    $"Cannot add the goal to {goalName} *{minorFactionName}* in {starSystemName}.",
                    $"The goal *{goalName}* does not exist.",
                    "Try again, checking the spelling and capitalization carefully.");
            }
            catch (Exception ex)
            {
                await Response.Exception(ex);
            }
        }

        [SlashCommand("remove", "Remove the specific goal for this minor faction in this system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Remove(
            [
                Summary("minor-faction", "The minor faction"),
                Autocomplete(typeof(GoalMinorFactionsAutocompleteHandler))
            ]
            string minorFactionName,
            [
                Summary("star-system", "The star system"),
                Autocomplete(typeof(GoalStarSystemsAutocompleteHandler))
            ]
            string starSystemName
        )
        {
            try
            {
                Api.RemoveGoal(minorFactionName, starSystemName);
                await Response.Success($"Remove goal for *{minorFactionName}* in {starSystemName}", true);
                TransactionScope.Complete();
            }
            catch (UnknownMinorFactionException ex)
            {
                await Response.Error(
                    $"Cannot remove the goal for *{minorFactionName}* in {starSystemName}.",
                    $"The minor faction *{ex.MinorFactionName}* does not exist.",
                    "Try again, checking the spelling and capitalization carefully.");
            }
            catch (UnknownStarSystemException ex)
            {
                await Response.Error(
                    $"Cannot remove the goal for *{minorFactionName}* in {starSystemName}.",
                    $"The star system *{ex.StarSystemName}* does not exist.",
                    "Try again, checking the spelling and capitalization carefully.");
            }
            catch (Exception ex)
            {
                await Response.Exception(ex);
            }
        }

        [SlashCommand("list", "List any specific goals per minor faction and per system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, MembersRole.RoleName, Group = "Permission")]
        public async Task List()
        {
            try
            {
                string result = string.Join(Environment.NewLine,
                    Api.ListGoals().Select(
                        dgssmfg => $"{dgssmfg.Goal} {dgssmfg.Presence.MinorFaction.Name} in {dgssmfg.Presence.StarSystem.Name}"));
                if (result.Length == 0)
                {
                    await Response.Information("No goals specified");
                }
                else
                {
                    await Response.File(result, $"{Context.Guild.Name} Goals.txt");
                }
                TransactionScope.Complete();
            }
            catch (Exception ex)
            {
                await Response.Exception(ex);
            }
        }

        [SlashCommand("export", "Export the current goals for backup")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [RequireBotRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Export()
        {
            try
            {
                IList<GoalCsvRow> result = Api.ListGoals()
                    .Select(dgssmfg => new GoalCsvRow()
                    {
                        Goal = dgssmfg.Goal,
                        MinorFaction = dgssmfg.Presence.MinorFaction.Name,
                        StarSystem = dgssmfg.Presence.StarSystem.Name
                    })
                    .ToList();
                if (result.Count == 0)
                {
                    await Response.Information("No goals specified");
                }
                else
                {
                    await Response.CsvFile(result, $"{Context.Guild.Name} Goals.txt");
                }
            }
            catch (Exception ex)
            {
                await Response.Exception(ex);
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

                await Api.AddGoals(goals.Select(g => (g.MinorFaction, g.StarSystem, g.Goal)));
                AuditLogger.Audit($"Imported goals:\n{string.Join("\n", goals.Select(g => $"{g.Goal} {g.MinorFaction} in {g.StarSystem}"))}");
                await Response.Success($"{goalsAttachement.Filename} added to goals", false);
                TransactionScope.Complete();
            }
            catch (CsvHelperException)
            {
                await Response.Error(
                    "Cannot import the goals from the file.",
                    $"{goalsAttachement.Filename} is not a valid goals file.",
                    "Correct the file then import it again.");
            }
            catch (UnknownMinorFactionException ex)
            {
                await Response.Error(
                    "Cannot import the goals from the file.",
                    $"The minor faction *{ex.MinorFactionName}* does not exist.",
                    "Correct the file then import it again.");
            }
            catch (UnknownStarSystemException ex)
            {
                await Response.Error(
                    "Cannot import the goals from the file.",
                    $"The star system *{ex.StarSystemName}* does not exist.",
                    "Correct the file then import it again.");
            }
            catch (UnknownGoalException ex)
            {
                await Response.Error(
                    "Cannot import the goals from the file.",
                    $"The goal *{ex.Goal}* does not exist.",
                    "Correct the file the import it again.");
            }
            catch (Exception ex)
            {
                await Response.Exception(ex);
            }
        }
    }
}
