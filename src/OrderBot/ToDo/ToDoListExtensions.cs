﻿using Microsoft.Extensions.DependencyInjection;
using OrderBot.Discord;
using OrderBot.MessageProcessors;

namespace OrderBot.ToDo;

internal static class ToDoListExtensions
{
    internal static void AddTodoList(this IServiceCollection services)
    {
        services.AddSingleton<INameValidator, EliteBgsValidator>();
        services.AddSingleton<SupportedMinorFactionsCache>();
        services.AddSingleton<GoalStarSystemsCache>();
        services.AddSingleton<ToDoListApiFactory>();
        services.AddSingleton<ResultFactory>();
        services.AddScoped<EddnMessageProcessor, ToDoListMessageProcessor>();
    }
}
