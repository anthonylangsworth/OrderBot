﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.Discord;

internal static class BotExtensions
{
    // const string DiscordApiKeyEnvironmentVariable = "Discord__ApiKey";

    /// <summary>
    /// Add the Discord Bot service(s).
    /// </summary>
    /// <param name="services">
    /// Add services to this collection.
    /// </param>
    /// <param name="configuration">
    /// Configuration source.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Configuration is missing or invalid.
    /// </exception>
    public static void AddDiscordBot(this IServiceCollection services)
    {
        //string discordApiKey = configuration.GetRequiredSection(DiscordApiKeyEnvironmentVariable).Value;
        //if (string.IsNullOrEmpty(discordApiKey))
        //{
        //    throw new InvalidOperationException(
        //        $"Missing Discord API Key in environment variable `{DiscordApiKeyEnvironmentVariable}`.");
        //}
        services.AddSingleton(sp => new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = BotHostedService.Intents
        }));
        services.AddSingleton<IDiscordClient, DiscordSocketClient>();
        services.AddSingleton(sp => new InteractionService(
            sp.GetRequiredService<DiscordSocketClient>(),
            new InteractionServiceConfig()
            {
                DefaultRunMode = RunMode.Sync // Default is Async. Sync provides better error reporting.
            }));
        services.AddHostedService<BotHostedService>();
    }
}
