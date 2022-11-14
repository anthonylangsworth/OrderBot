﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    /// <param name="apiKey"></param>
    public BotHostedService(ILogger<BotHostedService> logger, DiscordSocketClient discordClient,
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
    /// The current <see cref="SocketInteractionContext"/>, or <see cref="null"/> if there is none.
    /// </summary>
    public static AsyncLocal<SocketInteractionContext> CurrentContext { get; } =
        new AsyncLocal<SocketInteractionContext>();

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
        if (Client.ConnectionState != ConnectionState.Disconnected)
        {
            throw new InvalidOperationException("Not disconnected");
        }

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
#pragma warning disable CA2254
        // TODO: Convert message.Severity
        Logger.LogInformation(message.ToString());
#pragma warning restore CA2254
        return Task.CompletedTask;
    }

    private async Task Client_InteractionCreated(SocketInteraction interaction)
    {
        using IServiceScope serviceScope = ServiceProvider.CreateScope();

        string errorMessage = null!;
        SocketInteractionContext context = new(Client, interaction);

        // Get an ILogger from the scope.
        ILogger<BotHostedService> logger = ServiceProvider.GetRequiredService<ILogger<BotHostedService>>();
        using IDisposable loggerScope = logger.BeginScope(new ScopeBuilder(context).Build());
        logger.LogInformation("Started");

        // Only tested with slash commands and autocomplete. May need excluding from other
        // interaction types.
        if (interaction is not IAutocompleteInteraction)
        {
            await interaction.DeferAsync(ephemeral: true);
        }
        IResult result = await InteractionService.ExecuteCommandAsync(context, ServiceProvider);
        if (result.IsSuccess)
        {
            Logger.LogInformation("Completed");
        }
        else
        {
            if (result.Error == InteractionCommandError.UnmetPrecondition)
            {
                errorMessage = $"**Error**: You do not have access to run this command";
                Logger.LogWarning("Error: You do not have access to run this command");
            }
            else
            {
                errorMessage = $"**Error**: Command failed. It is not you, it's me.";
                Logger.LogError("Error: {ErrorMessage}", result.ErrorReason);
            }
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            await interaction.FollowupAsync(text: errorMessage, ephemeral: true);
        }
    }
}
