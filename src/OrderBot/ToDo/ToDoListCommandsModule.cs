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

namespace OrderBot.ToDo
{
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
        // [RequirePerGuildRole("EDAKL Leaders", "EDAKL Veterans")]
        public async Task ShowToDoList()
        {
            await Context.Interaction.DeferAsync(ephemeral: true);
            using (Logger.BeginScope("show", Context.Guild.Name))
            {
                const string minorFactionName = "EDA Kunti League";
                await Context.Interaction.FollowupAsync(
                    text: Formatter.Format(Generator.Generate(Context.Guild.Id, minorFactionName)),
                    ephemeral: true
                );
            }
        }

        [SlashCommand("raw", "List the work required for supporting a minor faction in a copyable format")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        // [RequirePerGuildRole("EDAKL Leaders", "EDAKL Veterans")]
        public async Task ShowRawToDoList()
        {
            await Context.Interaction.DeferAsync(ephemeral: true);
            using (Logger.BeginScope("raw", Context.Guild.Name))
            {
                const string minorFactionName = "EDA Kunti League";
                await Context.Interaction.FollowupAsync(
                    text: $"```\n" +
                        $"{Formatter.Format(Generator.Generate(Context.Guild.Id, minorFactionName))}\n" +
                        $"```",
                    ephemeral: true
                );
            }
        }

        [Group("support", "Support a minor faction")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        public class Support : InteractionModuleBase<SocketInteractionContext>
        {
            public Support(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<Support> logger,
                DiscordChannelAuditLoggerFactory auditLogFactory)
            {
                ContextFactory = contextFactory;
                Logger = logger;
                AuditLogFactory = auditLogFactory;
            }

            public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
            public ILogger<Support> Logger { get; }
            public DiscordChannelAuditLoggerFactory AuditLogFactory { get; }

            [SlashCommand("add", "Start supporting this minor faction")]
            public async Task Add(
                [Summary("minor-faction", "Start supporting this minor faction")]
                string name
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                using (Logger.BeginScope(("Add", Context.Guild.Name, name)))
                {
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
            }

            [SlashCommand("remove", "Stop supporting this minor faction")]
            public async Task Remove(
                [Summary("minor-faction", "Stop supporting this minor faction")]
                string name
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                using (Logger.BeginScope(("Remove", Context.Guild.Name, name)))
                {
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
            }

            [SlashCommand("list", "List supported minor factions")]
            public async Task List()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using (Logger.BeginScope(("List", Context.Guild.Name)))
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
        }

        [Group("goal", "Provide specific intent for a minor faction in a system")]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
        public class Goals : InteractionModuleBase<SocketInteractionContext>
        {
            public Goals(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<Goals> logger,
                DiscordChannelAuditLoggerFactory auditLogFactory)
            {
                ContextFactory = contextFactory;
                Logger = logger;
                AuditLogFactory = auditLogFactory;
            }

            public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
            public ILogger<Goals> Logger { get; }
            public DiscordChannelAuditLoggerFactory AuditLogFactory { get; }

            [SlashCommand("add", "Set a specific goal for this minor faction in this system")]
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
                await Context.Interaction.DeferAsync(ephemeral: true);
                using (Logger.BeginScope(("Add", Context.Guild.Name, minorFactionName, starSystemName, goalName)))
                {
                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                    using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                    AddImplementation(dbContext, Context.Guild, new[] { (minorFactionName, starSystemName,
                        goalName) }, auditLogger);
                    await Context.Interaction.FollowupAsync(
                        text: $"**Success**! Goal {goalName} for *{minorFactionName}* in {starSystemName} added",
                        ephemeral: true
                    );
                }
            }

            internal static void AddImplementation(OrderBotDbContext dbContext, IGuild guild,
                IReadOnlyList<(string minorFactionName, string starSystemName, string goalName)> goals, IAuditLogger auditLogger)
            {
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
                        throw new ArgumentException($"{minorFactionName} is not a known goal");
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
                    auditLogger.Audit($"{goalName} {minorFactionName} in {starSystemName}");
                }
                dbContext.SaveChanges();
            }

            [SlashCommand("remove", "Remove the specific goal for this minor faction in this system")]
            public async Task Remove(
                [Summary("minor-faction", "The minor faction")]
                string minorFactionName,
                [Summary("star-system", "The star system")]
                string starSystemName
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                using (Logger.BeginScope(("Remove", Context.Guild.Name, minorFactionName, starSystemName)))
                {
                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                    RemoveImplementation(dbContext, Context.Guild, minorFactionName, starSystemName);
                    auditLogger.Audit($"Removed goal for {minorFactionName} in {starSystemName}");
                    await Context.Interaction.FollowupAsync(
                        text: $"**Success**! Goal for *{minorFactionName}* in {starSystemName} removed",
                        ephemeral: true
                    );
                }
            }

            internal static void RemoveImplementation(OrderBotDbContext dbContext, IGuild guild, string minorFactionName,
                string starSystemName)
            {
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
            }

            [SlashCommand("list", "List any specific goals per minor faction and per system")]
            public async Task List()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using (Logger.BeginScope(("List", Context.Guild.Name)))
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
            public async Task Export()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using (Logger.BeginScope(("Export", Context.Guild.Name)))
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
            }

            [SlashCommand("import", "Import new goals")]
            public async Task Import(
                [Summary("goals", "Export output: CSV with goal, minor faction and star system")]
                IAttachment goalsAttachement
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                using IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
                using (Logger.BeginScope(("Import", Context.Guild.Name, goalsAttachement.Url)))
                {
                    try
                    {
                        using HttpClient client = new();
                        using Stream stream = await client.GetStreamAsync(goalsAttachement.Url);
                        using StreamReader reader = new(stream);
                        using CsvReader csvReader = new(reader, CultureInfo.InvariantCulture);
                        IList<GoalCsvRow> goals = await csvReader.GetRecordsAsync<GoalCsvRow>().ToListAsync();

                        using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                        using (TransactionScope transactionScope = new())
                        {
                            foreach (GoalCsvRow row in goals)
                            {
                                AddImplementation(dbContext, Context.Guild,
                                    new[] { (row.MinorFaction, row.StarSystem, row.Goal) },
                                    auditLogger);
                            }
                            transactionScope.Complete();
                        }

                        await Context.Interaction.FollowupAsync(
                                text: $"**Success**! {goalsAttachement.Filename} added to goals",
                                ephemeral: true
                        );
                    }
                    catch (CsvHelperException ex)
                    {
                        throw new ArgumentException($"**Error**: {goalsAttachement.Filename} is not a valid goals file", ex);
                    }
                }
            }
        }
    }
}