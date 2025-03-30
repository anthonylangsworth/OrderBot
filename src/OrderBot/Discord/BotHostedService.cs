using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderBot.EntityFramework;
using System.Reflection;

namespace OrderBot.Discord;

/// <summary>
/// A Discord bot.
/// </summary>
internal class BotHostedService : IHostedService
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
    /// <param name="config">
    /// Configuration.
    /// </param>
    public BotHostedService(ILogger<BotHostedService> logger, DiscordSocketClient discordClient,
        InteractionService interactionService, IServiceProvider serviceProvider,
        IDbContextFactory<OrderBotDbContext> contextFactory, IOptions<DiscordClientOptions> config)
    {
        if (string.IsNullOrWhiteSpace(config.Value.ApiKey))
        {
            throw new ArgumentException("Discord API Key cannot be null, empty or whitespace", nameof(config));
        }

        Logger = logger;
        Client = discordClient;
        InteractionService = interactionService;
        ServiceProvider = serviceProvider;
        ContextFactory = contextFactory;
        ApiKey = config.Value.ApiKey;

        Client.Log += LogAsync;
        InteractionService.Log += LogAsync;
        Client.InteractionCreated += Client_InteractionCreated;
        Client.Ready += Client_ReadyAsync;
    }

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
    internal ILogger<BotHostedService> Logger { get; }

    /// <summary>
    /// Start.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException">
    /// Connection state must be disconnected.
    /// </exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InteractionService.AddModulesAsync(Assembly.GetExecutingAssembly(), ServiceProvider);

        await Client.LoginAsync(TokenType.Bot, ApiKey);
        await Client.StartAsync();

        Logger.LogInformation("Started");
    }

    /// <summary>
    /// Stop.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Stopped");
        await Client.StopAsync();
    }

    private async Task Client_ReadyAsync()
    {
        foreach (SocketGuild guild in Client.Guilds)
        {
            await InteractionService.RegisterCommandsToGuildAsync(guild.Id);
            await Client.Rest.DeleteAllGlobalCommandsAsync();
            using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
            DiscordHelper.GetOrAddGuild(dbContext, guild);

            Logger.LogInformation("Guild {Guild} ({GuildId}) added and commands registered", guild.Name, guild.Id);
        }

        await Client.SetActivityAsync(new Game("BGS", ActivityType.Watching));
    }

    private Task LogAsync(LogMessage message)
    {
        LogLevel logLevel = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Debug => LogLevel.Debug,
            LogSeverity.Verbose => LogLevel.Trace,
            _ => LogLevel.Critical // Be pessimistic
        };

        // Log message conversion requires accessing variables
#pragma warning disable CA2254
        if (message.Exception != null)
        {
            // Override for reconnects, as this is processed within Discord.Net code
            if (message.Exception is GatewayReconnectException)
            {
                logLevel = LogLevel.Information;
            }
            Logger.Log(logLevel, message.Exception, message.ToString());
        }
        else
        {
            Logger.Log(logLevel, message.ToString());
        }
#pragma warning restore
        return Task.CompletedTask;
    }

    private async Task Client_InteractionCreated(SocketInteraction interaction)
    {
        using IServiceScope serviceScope = ServiceProvider.CreateScope();

        string errorMessage = null!;
        SocketInteractionContext context = new(Client, interaction);

        // Get an ILogger from the scope.
        ILogger<BotHostedService> logger = ServiceProvider.GetRequiredService<ILogger<BotHostedService>>();
        using IDisposable? loggerScope = logger.BeginScope(new InteractionScopeBuilder(context).Build());

        IResult result = await InteractionService.ExecuteCommandAsync(context, ServiceProvider);
        if (!result.IsSuccess)
        {
            const string internalErrorMessage = "Command failed. It's not you, it's me. The error has been logged for review.";
            if (result is PreconditionResult)
            {
                errorMessage = $"You lack the permission to run this command. Contact your Discord admins if you think this is incorrect.";
                Logger.LogWarning("Unmet precondition (e.g. access denied)");
            }
            else if (result is ExecuteResult executeResult)
            {
                errorMessage = internalErrorMessage;
                Logger.LogError(executeResult.Exception, "Unhandled exception");
            }
            else
            {
                errorMessage = internalErrorMessage;
                Logger.LogError("Error: {ErrorMessage}", result.ErrorReason);
            }

            await context.Channel.SendMessageAsync(errorMessage, flags: MessageFlags.Ephemeral);
        }
    }
}
