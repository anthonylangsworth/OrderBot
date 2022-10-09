using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderBot;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.MessageProcessors;
using OrderBot.Reports;

const string environmentVariablePrefix = ""; // "OB__"
const string databaseEnvironmentVariable = "OrderBot";
const string discordApiKeyEnvironmentVariable = "DiscordApiKey";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddEnvironmentVariables(environmentVariablePrefix))
    .ConfigureServices((hostContext, services) =>
    {
        // Database Connection
        string dbConnectionString = hostContext.Configuration.GetConnectionString(databaseEnvironmentVariable);
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new InvalidOperationException(
                $"Database connection string missing from environment variable `{environmentVariablePrefix}ConnectionStrings__{databaseEnvironmentVariable}`. " +
                "Usually in the form of `Server=server;Database=OrderBot;User ID=OrderBot;Password=password`.");
        }
        services.AddDbContextFactory<OrderBotDbContext>(
            dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(dbConnectionString)); // "Server=localhost;Database=OrderBot;User ID=OrderBot;Password=password"
                                                                                                  // options => options.EnableRetryOnFailure()));

        // Report generation
        services.AddSingleton<ToDoListGenerator>();
        services.AddSingleton<ToDoListFormatter>();

        // EDDN Message Processor
        services.AddSingleton<MinorFactionNameFilter, FixedMinorFactionNameFilter>(sp => new FixedMinorFactionNameFilter(new[] { "EDA Kunti League" }));
        services.AddSingleton<EddnMessageProcessor, SystemMinorFactionMessageProcessor>();
        services.AddHostedService<EddnMessageBackgroundService>();

        // Discord Bot
        string discordApiKey = hostContext.Configuration.GetRequiredSection(discordApiKeyEnvironmentVariable).Value;
        if (string.IsNullOrEmpty(discordApiKey))
        {
            throw new InvalidOperationException(
                $"Missing Discord API Key in environment variable `{discordApiKeyEnvironmentVariable}`.");
        }
        services.AddSingleton(sp => new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = BotBackgroundService.Intents
        }));
        services.AddSingleton<InteractionService>();
        services.AddSingleton(new InteractionServiceConfig()
        {
            DefaultRunMode = RunMode.Sync
        });
        services.AddHostedService<BotBackgroundService>(
            sp => new(sp.GetRequiredService<ILogger<BotBackgroundService>>(),
                sp.GetRequiredService<DiscordSocketClient>(),
                sp.GetRequiredService<InteractionService>(),
                sp,
                sp.GetRequiredService<IDbContextFactory<OrderBotDbContext>>(),
                discordApiKey));
    })
    .Build();

await host.RunAsync();