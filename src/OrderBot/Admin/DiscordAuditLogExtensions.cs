﻿using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.Admin
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
        public static void AddDiscordChannelAuditor(this IServiceCollection services)
        {
            services.AddSingleton<DiscordChannelAuditLog>();
        }
    }
}
