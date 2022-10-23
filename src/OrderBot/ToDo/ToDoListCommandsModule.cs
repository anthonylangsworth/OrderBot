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
    }
}