using Microsoft.Extensions.DependencyInjection;
using OrderBot.MessageProcessors;

namespace OrderBot.ToDo;

internal static class ToDoListExtensions
{
    internal static void AddTodoList(this IServiceCollection services)
    {
        services.AddSingleton<ToDoListApiFactory>();
        services.AddScoped<EddnMessageProcessor, ToDoListMessageProcessor>();
    }
}
