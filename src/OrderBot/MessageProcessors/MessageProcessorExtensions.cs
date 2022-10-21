using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.MessageProcessors
{
    internal static class MessageProcessorExtensions
    {
        internal static void AddBaseMessageProcessor(this IServiceCollection services)
        {
            // Multiple registrations do nothing
            services.AddHostedService<EddnMessageBackgroundService>();
        }
    }
}
