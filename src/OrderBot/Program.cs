using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderBot.CarrierMovement;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;
using OrderBot.ToDo;
using Serilog;

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
                            if(loggingConfig == null)
                            {
                                 throw new InvalidOperationException("LogAnalytics configuration section missing");
                            }

                            Serilog.ILogger serilogLogger = new LoggerConfiguration()
                                .WriteTo.AzureAnalytics(loggingConfig.WorkspaceId, loggingConfig.WorkspaceKey, logName: "Event_CL")
                                .CreateLogger();

                            services.AddLogging(builder => builder.AddSerilog(serilogLogger, dispose:true));
                            services.AddMemoryCache();

                            services.AddDatabase(hostContext.Configuration);
                            services.AddTodoList();
                            services.AddDiscordBot(hostContext.Configuration);
                            services.AddCarrierMovement();

                            // This must follow AddDiscordBot. Otherwise, the BotHostedService.StartAsync does
                            // not fire. This maybe something to do with the use of BackgroundService instead
                            // of IHostedService.
                            // TODO: Fix this
                            services.AddEddnMessageProcessor();
                        })
                         .Build();

        await host.RunAsync();
    }
}
