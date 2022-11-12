using CsvHelper;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Audit;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.Rbac;
using System.Globalization;
using System.Text;
using System.Transactions;

namespace OrderBot.ToDo;

[Group("todo-list", "Work supporting minor faction(s)")]
public class ToDoListCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    public ToDoListCommandsModule(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<ToDoListCommandsModule> logger,
        ToDoListGenerator generator, ToDoListFormatter formatter)
    {
        ContextFactory = contextFactory;
        Logger = logger;
        Generator = generator;
        Formatter = formatter;
    }

    public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
    public ILogger<ToDoListCommandsModule> Logger { get; }
    public ToDoListGenerator Generator { get; }
    public ToDoListFormatter Formatter { get; }

    [SlashCommand("show", "List the work required for supporting a minor faction")]
    [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
    [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
    [Discord.RequireRole(MembersRole.RoleName, Group = "Permission")]
    public async Task ShowToDoList()
    {
        const string minorFactionName = "EDA Kunti League";
        await Context.Interaction.FollowupAsync(
            text: Formatter.Format(Generator.Generate(Context.Guild.Id, minorFactionName)),
            ephemeral: true
        );
    }

    [SlashCommand("raw", "List the work required for supporting a minor faction in a copyable format")]
    [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
    [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
    public async Task ShowRawToDoList()
    {
        const string minorFactionName = "EDA Kunti League";
        await Context.Interaction.FollowupAsync(
            text: $"```\n" +
                $"{Formatter.Format(Generator.Generate(Context.Guild.Id, minorFactionName))}\n" +
                $"```",
            ephemeral: true
        );
    }

    [Group("support", "Support a minor faction")]
    public class Support : InteractionModuleBase<SocketInteractionContext>
    {
        public Support(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<Support> logger,
            TextChannelAuditLoggerFactory auditLogFactory)
        {
            ContextFactory = contextFactory;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        public ILogger<Support> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }

        [SlashCommand("add", "Start supporting this minor faction")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Add(
            [Summary("minor-faction", "Start supporting this minor faction")]
            string name
        )
        {
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            string message;
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(mf => mf.Name == name);
            if (minorFaction == null)
            {
                message = $"**Error**: {name} is not a known minor faction";
            }
            else
            {
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild,
                    dbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
                if (!discordGuild.SupportedMinorFactions.Contains(minorFaction))
                {
                    discordGuild.SupportedMinorFactions.Add(minorFaction);
                }
                message = $"**Success**! Now supporting *{minorFaction.Name}*";
                auditLogger.Audit($"Support minor faction '{name}'");
            }
            dbContext.SaveChanges();
            await Context.Interaction.FollowupAsync(
                    text: message,
                    ephemeral: true
            );
        }

        [SlashCommand("remove", "Stop supporting this minor faction")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Remove(
            [Summary("minor-faction", "Stop supporting this minor faction")]
            string name
        )
        {
            using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
            string message;
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(mf => mf.Name == name);
            if (minorFaction == null)
            {
                message = $"**Error**: {name} is not a known minor faction";
            }
            else
            {
                DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild,
                    dbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
                if (discordGuild.SupportedMinorFactions.Contains(minorFaction))
                {
                    discordGuild.SupportedMinorFactions.Remove(minorFaction);
                }
                message = $"**Success**! **NOT** supporting *{minorFaction.Name}*";
                auditLogger.Audit($"Stop supporting minor faction '{name}'");
            }
            dbContext.SaveChanges();
            await Context.Interaction.FollowupAsync(
                    text: message,
                    ephemeral: true
            );
        }

        [SlashCommand("list", "List supported minor factions")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
        [Discord.RequireRole(MembersRole.RoleName, Group = "Permission")]
        public async Task List()
        {
            string message;
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild,
                dbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
            if (discordGuild.SupportedMinorFactions.Any())
            {
                message = string.Join(Environment.NewLine,
                                        discordGuild.SupportedMinorFactions.Select(mf => mf.Name));
            }
            else
            {
                message = $"No supported minor factions";
            }
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
            TextChannelAuditLoggerFactory auditLogFactory)
        {
            ContextFactory = contextFactory;
            Logger = logger;
            AuditLogFactory = auditLogFactory;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        public ILogger<Goals> Logger { get; }
        public TextChannelAuditLoggerFactory AuditLogFactory { get; }

        [SlashCommand("add", "Set a specific goal for this minor faction in this system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
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
                AddImplementation(dbContext, Context.Guild,
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

        internal static void AddImplementation(OrderBotDbContext dbContext, IGuild guild,
            IEnumerable<(string minorFactionName, string starSystemName, string goalName)> goals)
        {
            using TransactionScope transactionScope = new();

            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild);

            foreach ((string minorFactionName, string starSystemName, string goalName) in goals)
            {
                MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(mf => mf.Name == minorFactionName);
                if (minorFaction == null)
                {
                    throw new ArgumentException($"*{minorFactionName}* is not a known minor faction");
                }

                StarSystem? starSystem = dbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
                if (starSystem == null)
                {
                    throw new ArgumentException($"{starSystemName} is not a known star system");
                }

                if (!ToDo.Goals.Map.TryGetValue(goalName, out Goal? goal))
                {
                    throw new ArgumentException($"{goalName} is not a known goal");
                }

                Presence? starSystemMinorFaction =
                    dbContext.Presences.Include(ssmf => ssmf.StarSystem)
                                       .Include(ssmf => ssmf.MinorFaction)
                                       .FirstOrDefault(ssmf => ssmf.StarSystem.Name == starSystemName
                                                            && ssmf.MinorFaction.Name == minorFactionName);
                if (starSystemMinorFaction == null)
                {
                    starSystemMinorFaction = new Presence() { MinorFaction = minorFaction, StarSystem = starSystem };
                    dbContext.Presences.Add(starSystemMinorFaction);
                }

                DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
                    dbContext.DiscordGuildPresenceGoals
                             .Include(dgssmfg => dgssmfg.Presence)
                             .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                             .Include(dgssmfg => dgssmfg.Presence.MinorFaction)
                             .FirstOrDefault(
                                 dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                         && dgssmfg.Presence.MinorFaction == minorFaction
                                         && dgssmfg.Presence.StarSystem == starSystem);
                if (discordGuildStarSystemMinorFactionGoal == null)
                {
                    discordGuildStarSystemMinorFactionGoal = new DiscordGuildPresenceGoal()
                    { DiscordGuild = discordGuild, Presence = starSystemMinorFaction };
                    dbContext.DiscordGuildPresenceGoals.Add(discordGuildStarSystemMinorFactionGoal);
                }
                discordGuildStarSystemMinorFactionGoal.Goal = goalName;
                dbContext.SaveChanges();
            }
            transactionScope.Complete();
        }

        [SlashCommand("remove", "Remove the specific goal for this minor faction in this system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
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
                RemoveImplementation(dbContext, Context.Guild, minorFactionName, starSystemName);
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

        internal static void RemoveImplementation(OrderBotDbContext dbContext, IGuild guild, string minorFactionName,
            string starSystemName)
        {
            using TransactionScope transactionScope = new();
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild);

            MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(mf => mf.Name == minorFactionName);
            if (minorFaction == null)
            {
                throw new ArgumentException($"*{minorFactionName}* is not a known minor faction");
            }

            StarSystem? starSystem = dbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
            if (starSystem == null)
            {
                throw new ArgumentException($"{starSystemName} is not a known star system");
            }

            DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
                dbContext.DiscordGuildPresenceGoals
                            .Include(dgssmfg => dgssmfg.Presence)
                            .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                            .Include(dgssmfg => dgssmfg.Presence.MinorFaction)
                            .FirstOrDefault(
                                dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                        && dgssmfg.Presence.MinorFaction == minorFaction
                                        && dgssmfg.Presence.StarSystem == starSystem);
            if (discordGuildStarSystemMinorFactionGoal != null)
            {
                dbContext.DiscordGuildPresenceGoals.Remove(discordGuildStarSystemMinorFactionGoal);
            }
            dbContext.SaveChanges();
            transactionScope.Complete();
        }

        [SlashCommand("list", "List any specific goals per minor faction and per system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
        [Discord.RequireRole(MembersRole.RoleName, Group = "Permission")]
        public async Task List()
        {
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            string result = string.Join(Environment.NewLine,
                ListImplementation(dbContext, Context.Guild).Select(
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

        internal static IEnumerable<DiscordGuildPresenceGoal> ListImplementation(OrderBotDbContext dbContext, IGuild guild)
        {
            DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild);
            return dbContext.DiscordGuildPresenceGoals
                            .Include(dgssmfg => dgssmfg.Presence)
                            .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                            .Include(dgssmfg => dgssmfg.Presence.MinorFaction);
        }

        [SlashCommand("export", "Export the current goals for backup")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles, Group = "Permission")]
        [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
        public async Task Export()
        {
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            IList<GoalCsvRow> result =
                ListImplementation(dbContext, Context.Guild)
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
        [Discord.RequireRole(OfficersRole.RoleName, Group = "Permission")]
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
                AddImplementation(dbContext, Context.Guild,
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
