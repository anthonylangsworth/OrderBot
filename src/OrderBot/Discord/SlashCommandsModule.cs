using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using OrderBot.Core;
using OrderBot.Reports;

namespace OrderBot.Discord
{
    public class SlashCommandsModule : InteractionModuleBase<InteractionContext>
    {
        internal SlashCommandsModule(IDbContextFactory<OrderBotDbContext> contextFactory, ToDoListGenerator generator, ToDoListFormatter formatter)
        {
            ContextFactory = contextFactory;
            Generator = generator;
            Formatter = formatter;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        internal ToDoListGenerator Generator { get; }
        internal ToDoListFormatter Formatter { get; }

        // public async Task

        [SlashCommand("todo-list", "List the work required for supporting a minor faction")]
        [RequireUserPermission(GuildPermission.ManageRoles | GuildPermission.ManageChannels)]
        public async Task ToDoList([Summary("True if the data is quoted, allowing easy coping, false (default) if formatted")] bool raw = false)
        {
            await Context.Interaction.DeferAsync(ephemeral: true);

            try
            {
                // Context.Guild is null for some reason
                SocketGuild guild = ((SocketGuildUser)Context.User).Guild;

                const string minorFactionName = "EDA Kunti League";
                string report = Formatter.Format(Generator.Generate(guild.Id.ToString(), minorFactionName));

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

        public static string GetDisplayName(IGuildUser user)
        {
            return user.Nickname ?? user.Username;
        }
    }
}