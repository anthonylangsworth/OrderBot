using Microsoft.Extensions.DependencyInjection;
using OrderBot.MessageProcessors;

namespace OrderBot.ToDo;

internal static class ToDoExtensions
{
    internal static void AddTodoList(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ToDoListApiFactory>();
        services.AddSingleton<EddnMessageProcessor, ToDoListMessageProcessor>();
    }
}
