using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrderBot.Admin;
using OrderBot.CarrierMovement;
using OrderBot.Discord;
using OrderBot.EntityFramework;
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
                             services.AddEddnMessageProcessor();
                             services.AddTodoList();
                             services.AddCarrierMovement();
                             services.AddDiscordBot(hostContext.Configuration);
                             services.AddDiscordChannelAuditLogFactory();
                         })
                         .Build();

        await host.RunAsync();
    }
}