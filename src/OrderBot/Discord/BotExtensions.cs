using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.Discord;

internal static class BotExtensions
{
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
        DiscordSocketClient discordSocketClient = new(new DiscordSocketConfig()
        {
            GatewayIntents = BotHostedService.Intents
        });

        services.AddSingleton(discordSocketClient);
        services.AddSingleton<IDiscordClient>(discordSocketClient);
        services.AddSingleton(sp => new InteractionService(
            discordSocketClient,
            new InteractionServiceConfig()
            {
                DefaultRunMode = RunMode.Sync // Catch and handle errors
            }));

        services.AddSingleton<TextChannelWriterFactory>();

        services.AddOptions<DiscordClientOptions>()
                .Bind(configuration.GetRequiredSection("Discord"));
        services.AddHostedService<BotHostedService>();
    }
}
