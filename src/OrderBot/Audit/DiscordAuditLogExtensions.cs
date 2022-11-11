using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.Audit
{
    internal static class DiscordBotExtensions
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
        public static void AddDiscordChannelAuditLogFactory(this IServiceCollection services)
        {
            services.AddSingleton<TextChannelAuditLoggerFactory>();
        }
    }
}
