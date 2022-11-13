using Microsoft.Extensions.DependencyInjection;
using OrderBot.MessageProcessors;

namespace OrderBot.ToDo;

internal static class ToDoExtensions
{
    internal static void AddTodoList(this IServiceCollection services)
    {
        services.AddSingleton<ToDoListGenerator>();
        services.AddSingleton<ToDoListFormatter>();
        services.AddSingleton<ToDoListApi>();

        services.AddSingleton<MinorFactionNameFilter, FixedMinorFactionNameFilter>(sp => new FixedMinorFactionNameFilter(new[] { "EDA Kunti League" }));
        services.AddSingleton<EddnMessageProcessor, ToDoListMessageProcessor>();
    }
}
