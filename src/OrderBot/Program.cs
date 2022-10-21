using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.MessageProcessors;
using OrderBot.ToDo;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
                         .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddEnvironmentVariables())
                         .ConfigureServices((hostContext, services) =>
                         {
                             services.AddDatabase(hostContext.Configuration);
                             services.AddBaseMessageProcessor();
                             services.AddTodoList();
                             services.AddCarrierMovement();
                             services.AddDiscordBot(hostContext.Configuration);
                         })
                         .Build();

        await host.RunAsync();
    }
}