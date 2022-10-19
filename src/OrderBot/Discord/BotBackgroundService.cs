using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using System.Reflection;
using System.Transactions;

namespace OrderBot.Discord
{
    /// <summary>
    /// A Discord bot.
    /// </summary>
    internal class BotBackgroundService : IHostedService
    {
        /// <summary>
        /// Construct a <see cref="Bot"/>.
        /// </summaryq>
        /// <param name="logger">
        /// Log to here.
        /// </param>
        /// <param name="discordClient">
        /// The <see cref="DiscordSocketClient"/> to use.
        /// </param>
        /// <param name="interactionService">
        /// The <see cref="InteractionService"/> to handle slash commands.
        /// </param>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> to instantiate other classes.
        /// </param>
        /// <param name="contextFactory"></param>
        /// <param name="apiKey"></param>
        public BotBackgroundService(ILogger<BotBackgroundService> logger, DiscordSocketClient discordClient,
            InteractionService interactionService, IServiceProvider serviceProvider,
            IDbContextFactory<OrderBotDbContext> contextFactory, string apiKey)
        {
            Logger = logger;
            Client = discordClient;
            InteractionService = interactionService;
            ServiceProvider = serviceProvider;
            ContextFactory = contextFactory;
            ApiKey = apiKey;

            Client.Log += LogAsync;
            InteractionService.Log += LogAsync;
            Client.SlashCommandExecuted += Client_SlashCommandExecutedAsync;
            Client.AutocompleteExecuted += Client_AutocompleteExecuted;
            Client.Ready += Client_ReadyAsync;
        }

        // TODO: Confirm whether the GuildMembers intent is required or I just need to move the bot role
        // above most other roles.

        /// <summary>
        /// Discord features the bot uses, which must be requested up front.
        /// </summary>
        /// <remarks>
        /// The intent GatewayIntents.GuildMembers is required to get information about users, e.g. their roles.
        /// The intent GatewayIntents.Guilds is required to get information about roles.
        /// </remarks>
        public static GatewayIntents Intents => GatewayIntents.GuildMembers | GatewayIntents.Guilds;

        /// <summary>
        /// The Discord client.
        /// </summary>
        internal DiscordSocketClient Client { get; }
        internal InteractionService InteractionService { get; }
        internal IServiceProvider ServiceProvider { get; }
        internal IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        internal string ApiKey { get; }
        internal ILogger<BotBackgroundService> Logger { get; }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                throw new InvalidOperationException("Already started");
            }

            await InteractionService.AddModulesAsync(Assembly.GetExecutingAssembly(), ServiceProvider);

            await Client.LoginAsync(TokenType.Bot, ApiKey);
            await Client.StartAsync();

            Logger.LogInformation("Started");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync();
        }

        private async Task Client_ReadyAsync()
        {
            foreach (SocketGuild guild in Client.Guilds)
            {
                await InteractionService.RegisterCommandsToGuildAsync(guild.Id);
                AddDiscordGuild(ContextFactory, guild.Id);

                Logger.LogInformation("Guild {name} ({guildId}) added and commands registered", guild.Name, guild.Id);
            }
        }

        private Task LogAsync(LogMessage message)
        {
#pragma warning disable CA2254
            // TODO: Convert message.Severity
            Logger.LogInformation(message.ToString());
#pragma warning restore CA2254
            return Task.CompletedTask;
        }

        private async Task Client_SlashCommandExecutedAsync(SocketSlashCommand socketSlashCommand)
        {
            IResult result = await InteractionService.ExecuteCommandAsync(
                new SocketInteractionContext(Client, socketSlashCommand),
                ServiceProvider);
            if (!result.IsSuccess)
            {
                Logger.LogError("Command failed: {message}", result.ToString());
            }
            else
            {
                Logger.LogInformation("Command succeeded: {message}", socketSlashCommand.CommandName);
            }
        }

        private Task Client_AutocompleteExecuted(SocketAutocompleteInteraction arg)
        {
            SearchResult<AutocompleteCommandInfo> result = InteractionService.SearchAutocompleteCommand(arg);
            if (!result.IsSuccess)
            {
                Logger.LogError("Autocompletion failed: {message}", result.ToString());
            }
            else
            {
                Logger.LogInformation("Autocompletion succeeded");
            }
            return Task.CompletedTask;
        }

        internal static void AddDiscordGuild(IDbContextFactory<OrderBotDbContext> contextFactory, ulong guildId)
        {
            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            DiscordGuild? discordGuild = dbContext.DiscordGuilds.FirstOrDefault(dg => dg.GuildId == guildId);
            if (discordGuild == null)
            {
                dbContext.DiscordGuilds.Add(new DiscordGuild() { GuildId = guildId });
                dbContext.SaveChanges();
            }

            transactionScope.Complete();
        }
    }
}
