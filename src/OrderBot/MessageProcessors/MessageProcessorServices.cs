using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.MessageProcessors
{
    internal static class MessageProcessorServices
    {
        internal static void AddTodoListMessageProcessor(this IServiceCollection services)
        {
            services.AddSingleton<MinorFactionNameFilter, FixedMinorFactionNameFilter>(sp => new FixedMinorFactionNameFilter(new[] { "EDA Kunti League" }));
            services.AddSingleton<EddnMessageProcessor, ToDoListMessageProcessor>();
            services.AddBackgroundService();
        }

        internal static void AddCarrierMovementMessageProcessor(this IServiceCollection services)
        {
            services.AddSingleton<EddnMessageProcessor, CarrierMovementMessageProcessor>();
            services.AddBackgroundService();
        }

        private static void AddBackgroundService(this IServiceCollection services)
        {
            // Multiple registrations do nothing
            services.AddHostedService<EddnMessageBackgroundService>();
        }
    }
}
