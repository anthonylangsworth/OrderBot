using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.MessageProcessors;

internal static class EddnMessageProcessorExtensions
{
    internal static void AddEddnMessageProcessor(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHostedService<EddnMessageHostedService>();
    }
}
