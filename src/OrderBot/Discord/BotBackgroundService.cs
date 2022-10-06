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
    internal class BotBackgroundService : BackgroundService
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
        }

        /// <summary>
        /// Discord features the bot uses, which must be requested up front.
        /// </summary>
        public static GatewayIntents Intents => GatewayIntents.None;

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
        /// Start the bot.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The bot was already started.
        /// </exception>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                throw new InvalidOperationException("Already started");
            }

            Client.Log += LogAsync;
            InteractionService.Log += LogAsync;

            Client.SlashCommandExecuted += Client_SlashCommandExecutedAsync;
            Client.Ready += Client_ReadyAsync;

            await Client.LoginAsync(TokenType.Bot, ApiKey);
            await Client.StartAsync();

            Logger.LogInformation("Started");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task Client_ReadyAsync()
        {
            foreach (SocketGuild guild in Client.Guilds)
            {
                await InteractionService.AddModulesAsync(Assembly.GetExecutingAssembly(), ServiceProvider);
                await InteractionService.RegisterCommandsToGuildAsync(guild.Id);

                string guildId = guild.Id.ToString();
                AddDiscordGuild(ContextFactory, guildId);

                Logger.LogInformation("Guild {name} ({guildId}) added and commands registered", guild.Name, guildId);
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
            await InteractionService.ExecuteCommandAsync(
                new SocketInteractionContext(Client, socketSlashCommand),
                ServiceProvider);
        }

        internal static void AddDiscordGuild(IDbContextFactory<OrderBotDbContext> contextFactory, string guildId)
        {
            using OrderBotDbContext dbContext = contextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            DiscordGuild? discordGuild = dbContext.DiscordGuilds.Where(dg => dg.GuildId == guildId)
                                                                .FirstOrDefault();
            if (discordGuild == null)
            {
                dbContext.DiscordGuilds.Add(new DiscordGuild() { GuildId = guildId });
                dbContext.SaveChanges();
            }

            transactionScope.Complete();
        }
    }
}
