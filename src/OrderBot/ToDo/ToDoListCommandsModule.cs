using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;

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
    }
}