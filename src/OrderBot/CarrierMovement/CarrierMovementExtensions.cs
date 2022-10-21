using Microsoft.Extensions.DependencyInjection;
using OrderBot.MessageProcessors;

namespace OrderBot.CarrierMovement
{
    internal static class CarrierMovementExtensions
    {
        internal static void AddCarrierMovement(this IServiceCollection services)
        {
            services.AddSingleton<EddnMessageProcessor, CarrierMovementMessageProcessor>();
        }
    }
}
