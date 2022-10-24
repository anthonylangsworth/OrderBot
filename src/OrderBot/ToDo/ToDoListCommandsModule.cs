using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using OrderBot.Discord;

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
            // [Summary("raw", "True if the data is quoted, allowing easy coping, false (default) if formatted")] bool raw = false
            await Context.Interaction.DeferAsync(ephemeral: true);

            Logger.LogInformation("ToDoList called");

            try
            {
                // Context.Guild is null for some reason
                SocketGuild guild = ((SocketGuildUser)Context.User).Guild;

                const string minorFactionName = "EDA Kunti League";
                string report = Formatter.Format(Generator.Generate(guild.Id, minorFactionName));

                await Context.Interaction.FollowupAsync(
                    text: report,
                    ephemeral: true
                );
            }
            catch
            {
                await Context.Interaction.ModifyOriginalResponseAsync(messageProperties => messageProperties.Content = "I have failed.");
                throw;
            }
        }

        [Group("support", "Support a minor faction")]
        public class Support : InteractionModuleBase<SocketInteractionContext>
        {
            public Support(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<Support> logger)
            {
                ContextFactory = contextFactory;
                Logger = logger;
            }

            public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
            public ILogger<Support> Logger { get; }

            [SlashCommand("add", "Start supporting this minor faction")]
            public async Task Add(
                [Summary("minor-faction", "Start supporting this minor faction")]
                string name
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                string message;
                try
                {
                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                    MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(mf => mf.Name == name);
                    if (minorFaction == null)
                    {
                        message = $"{name} is not a known minor faction";
                    }
                    else
                    {
                        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild,
                            dbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
                        if (!discordGuild.SupportedMinorFactions.Contains(minorFaction))
                        {
                            discordGuild.SupportedMinorFactions.Add(minorFaction);
                        }
                        message = $"Now supporting *{minorFaction.Name}*";
                    }
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Add Failed");
                    message = "Failed";
                }
                await Context.Interaction.FollowupAsync(
                       text: message,
                       ephemeral: true
                );
            }

            [SlashCommand("remove", "Stop supporting this minor faction")]
            public async Task Remove(
                [Summary("minor-faction", "Stop supporting this minor faction")]
                string name
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                string message;
                try
                {
                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                    MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(mf => mf.Name == name);
                    if (minorFaction == null)
                    {
                        message = $"{name} is not a known minor faction";
                    }
                    else
                    {
                        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, Context.Guild,
                            dbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
                        if (discordGuild.SupportedMinorFactions.Contains(minorFaction))
                        {
                            discordGuild.SupportedMinorFactions.Remove(minorFaction);
                        }
                        message = $"**NOT** supporting *{minorFaction.Name}*";
                    }
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Remove Failed");
                    message = "Failed";
                }
                await Context.Interaction.FollowupAsync(
                       text: message,
                       ephemeral: true
                );
            }

            [SlashCommand("list", "List supported minor factions")]
            public async Task List()
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                string message;
                try
                {
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
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "List Failed");
                    message = "Failed";
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
            public Goals(IDbContextFactory<OrderBotDbContext> contextFactory, ILogger<Goals> logger)
            {
                ContextFactory = contextFactory;
                Logger = logger;
            }

            public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
            public ILogger<Goals> Logger { get; }

            [SlashCommand("add", "Start supporting this minor faction")]
            public async Task Add(
                [Summary("minor-faction", "Start supporting this minor faction")]
                string minorFactionName,
                [Summary("starSystem", "Start supporting this minor faction")]
                string starSystemName,
                [Summary("goal", "Start supporting this minor faction")]
                string goalName
            )
            {
                await Context.Interaction.DeferAsync(ephemeral: true);
                string message;
                try
                {
                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                    AddImplementation(dbContext, Context.Guild, minorFactionName, starSystemName, goalName);
                    message = $"Now supporting *{minorFactionName}*";
                }
                catch (ArgumentException ex)
                {
                    message = ex.Message;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Add Failed");
                    message = "Failed";
                }
                await Context.Interaction.FollowupAsync(
                       text: message,
                       ephemeral: true
                );
            }

            internal static void AddImplementation(OrderBotDbContext dbContext, IGuild guild, string minorFactionName, string starSystemName, string goalName)
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

                if (!ToDo.Goals.Map.TryGetValue(goalName, out Goal? goal))
                {
                    throw new ArgumentException($"{minorFactionName} is not a known goal");
                }

                StarSystemMinorFaction? starSystemMinorFaction =
                    dbContext.StarSystemMinorFactions.Include(ssmf => ssmf.StarSystem)
                                                     .Include(ssmf => ssmf.MinorFaction)
                                                     .FirstOrDefault(ssmf => ssmf.StarSystem.Name == starSystemName
                                                                          && ssmf.MinorFaction.Name == minorFactionName);
                if (starSystemMinorFaction == null)
                {
                    starSystemMinorFaction = new StarSystemMinorFaction() { MinorFaction = minorFaction, StarSystem = starSystem };
                    dbContext.StarSystemMinorFactions.Add(starSystemMinorFaction);
                }

                DiscordGuildStarSystemMinorFactionGoal? discordGuildStarSystemMinorFactionGoal =
                    dbContext.DiscordGuildStarSystemMinorFactionGoals
                                .Include(dgssmfg => dgssmfg.StarSystemMinorFaction)
                                .Include(dgssmfg => dgssmfg.StarSystemMinorFaction.StarSystem)
                                .Include(dgssmfg => dgssmfg.StarSystemMinorFaction.MinorFaction)
                                .FirstOrDefault(
                                dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                            && dgssmfg.StarSystemMinorFaction.MinorFaction == minorFaction
                                            && dgssmfg.StarSystemMinorFaction.StarSystem == starSystem);
                if (discordGuildStarSystemMinorFactionGoal == null)
                {
                    discordGuildStarSystemMinorFactionGoal = new DiscordGuildStarSystemMinorFactionGoal()
                    { DiscordGuild = discordGuild, StarSystemMinorFaction = starSystemMinorFaction };
                    dbContext.DiscordGuildStarSystemMinorFactionGoals.Add(discordGuildStarSystemMinorFactionGoal);
                }
                discordGuildStarSystemMinorFactionGoal.Goal = goalName;
                dbContext.SaveChanges();
            }
        }
    }
}