using Microsoft.Extensions.DependencyInjection;
using OrderBot.CarrierMovement;
using OrderBot.MessageProcessors;

namespace OrderBot.ToDo;

internal static class ToDoListExtensions
{
    internal static void AddTodoList(this IServiceCollection services)
    {
        services.AddSingleton<SupportedMinorFactionsCache>();
        services.AddSingleton<GoalStarSystemsCache>();
        services.AddSingleton<ToDoListApiFactory>();
        services.AddScoped<EddnMessageProcessor, ToDoListMessageProcessor>();
    }
}
