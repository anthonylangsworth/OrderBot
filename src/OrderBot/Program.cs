using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.MessageProcessors;
using OrderBot.Reports;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddEnvironmentVariables())
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDatabase(hostContext.Configuration);
        services.AddReports();
        services.AddTodoListMessageProcessor();
        services.AddCarrierMovementMessageProcessor();
        services.AddDiscordBot(hostContext.Configuration);
    })
    .Build();

await host.RunAsync();