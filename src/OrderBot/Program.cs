using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderBot.Audit;
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
                         .ConfigureServices((hostContext, services) =>
                         {
                             services.AddLogging(builder =>
                                builder.AddLogAnalytics(hostContext.Configuration.GetRequiredSection("LogAnalytics")));
                             services.AddDatabase(hostContext.Configuration);
                             services.AddTodoList();
                             services.AddDiscordBot(hostContext.Configuration);
                             services.AddDiscordChannelAuditLogFactory();
                             services.AddCarrierMovement();

                             // This must follow AddDiscordBot. Otherwise, the BotHostedServce.StartAsync does
                             // not fire. This maybe something to do with the use of BackgroundServce instead
                             // if IHostedSerice.
                             // TODO: Fix this
                             services.AddEddnMessageProcessor();
                         })
                         .Build();

        await host.RunAsync();
    }
}
