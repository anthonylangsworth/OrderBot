using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderBot.Core;

namespace OrderBot.Discord
{
    internal static class DiscordBotExtensions
    {

        const string DiscordApiKeyEnvironmentVariable = "DiscordApiKey";

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
        public static void AddDiscordBot(this IServiceCollection services, IConfiguration configuration)
        {
            string discordApiKey = configuration.GetRequiredSection(DiscordApiKeyEnvironmentVariable).Value;
            if (string.IsNullOrEmpty(discordApiKey))
            {
                throw new InvalidOperationException(
                    $"Missing Discord API Key in environment variable `{DiscordApiKeyEnvironmentVariable}`.");
            }
            services.AddSingleton(sp => new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = BotBackgroundService.Intents
            }));
            services.AddSingleton(sp => new InteractionService(
                sp.GetRequiredService<DiscordSocketClient>(),
                new InteractionServiceConfig()
                {
                    DefaultRunMode = RunMode.Sync // Default is Async. Sync provides better error reporting.
                }));
            services.AddHostedService<BotBackgroundService>(
                sp => new(sp.GetRequiredService<ILogger<BotBackgroundService>>(),
                    sp.GetRequiredService<DiscordSocketClient>(),
                    sp.GetRequiredService<InteractionService>(),
                    sp,
                    sp.GetRequiredService<IDbContextFactory<OrderBotDbContext>>(),
                    discordApiKey));
        }
    }
}
