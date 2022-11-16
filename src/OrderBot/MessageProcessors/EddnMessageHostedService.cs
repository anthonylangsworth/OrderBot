using Ionic.Zlib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System.Text;
using System.Text.Json;

namespace OrderBot.MessageProcessors;

internal class EddnMessageHostedService : BackgroundService
{
    public EddnMessageHostedService(ILogger<EddnMessageHostedService> logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    public ILogger<EddnMessageHostedService> Logger { get; }
    public IServiceProvider ServiceProvider { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using SubscriberSocket client = new("tcp://eddn.edcd.io:9500");
        client.SubscribeToAnyTopic();
        Logger.LogInformation("Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (client.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(1000), out byte[]? compressed, out bool _)
                && compressed != null)
            {
                await ProcessMessage(compressed);
            }
        }
    }

    internal async Task ProcessMessage(byte[] compressed)
    {
        using IServiceScope serviceScope = ServiceProvider.CreateScope();
        ILogger<EddnMessageHostedService> scopedLogger = ServiceProvider.GetRequiredService<ILogger<EddnMessageHostedService>>();
        IEnumerable<EddnMessageProcessor> messageProcessors = ServiceProvider.GetRequiredService<IEnumerable<EddnMessageProcessor>>();

        ScopeBuilder scopeBuilder = new ScopeBuilder();
        scopeBuilder.Add("UtcTime", DateTime.UtcNow);
        using (scopedLogger.BeginScope(scopeBuilder.Build()))
        {
            try
            {
                string message = Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(compressed));

                foreach (EddnMessageProcessor messageProcessor in messageProcessors)
                {
                    try
                    {
                        await messageProcessor.ProcessAsync(JsonDocument.Parse(message));
                    }
                    catch (JsonException)
                    {
                        scopedLogger.LogError("Invalid JSON", message);
                    }
                    catch (KeyNotFoundException)
                    {
                        scopedLogger.LogError("Required field(s) missing", message);
                    }
                    catch (FormatException)
                    {
                        scopedLogger.LogError("Incorrect field format", message);
                    }
                    catch (Exception ex)
                    {
                        scopedLogger.LogError(ex, "Process message failed");
                    }
                }
            }
            catch (ZlibException)
            {
                scopedLogger.LogError("Decompress message failed");
            }
        }
    }
}
