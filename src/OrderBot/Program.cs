using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderBot.CarrierMovement;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;
using OrderBot.ToDo;
using LogAnalytics.Extensions.Logging;
using Microsoft.Extensions.Logging;

public class LogAnalyticsConfigData
{
    public string? WorkspaceId { get; set; }
    public string? WorkspaceKey { get; set; }
}

internal class Program
{
    private static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
                         .ConfigureServices((hostContext, services) =>
                         {
                             LogAnalyticsConfigData? loggingConfig = hostContext.Configuration
                                 .GetRequiredSection("LogAnalytics")
                                 .Get<LogAnalyticsConfigData>();
                             if (loggingConfig == null)
                             {
                                 throw new InvalidOperationException("LogAnalytics configuration section missing");
                             }

                             services.AddLogging(builder => builder.Services.Add(
                                 ServiceDescriptor.Singleton<ILoggerProvider, LogAnalyticsLoggerProvider>(
                                     sp => new LogAnalyticsLoggerProvider(
                                        null,
                                        loggingConfig.WorkspaceId,
                                        loggingConfig.WorkspaceKey,
                                        "OrderBot",
                                        null))));
                             services.AddMemoryCache();

                             services.AddDatabase(hostContext.Configuration);
                             services.AddTodoList();
                             services.AddDiscordBot(hostContext.Configuration);
                             services.AddCarrierMovement();

                             // This must follow AddDiscordBot. Otherwise00, the BotHostedService.StartAsync does
                             // not fire. This maybe something to do with the use of BackgroundService instead
                             // of IHostedService.
                             // TODO: Fix this
                             services.AddEddnMessageProcessor();
                         })
                         .Build();

        await host.RunAsync();
    }
}
