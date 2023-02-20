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

    protected ILogger<EddnMessageHostedService> Logger { get; }
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Exclude Legacy events.
    /// </summary>
    public static Version RequiredGameVersion { get; } = new(4, 0);

    /// <summary>
    /// EDDN public queue.
    /// </summary>
    public readonly string ConnectionString = "tcp://eddn.edcd.io:9500";

    /// <summary>
    /// Recommend if no message received for this interval.
    /// </summary>
    public readonly TimeSpan ReconnectionInterval = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SubscriberSocket client = new(ConnectionString);
        try
        {
            client.SubscribeToAnyTopic();
            Logger.LogInformation("Started");

            DateTime lastMessageReceived = DateTime.Now;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (client.TryReceiveFrameBytes(TimeSpan.FromSeconds(5), out byte[]? compressed)
                    && compressed != null)
                {
                    lastMessageReceived = DateTime.Now;
                    await ProcessMessageAsync(compressed);
                }

                if (DateTime.Now - lastMessageReceived > ReconnectionInterval)
                {
                    Logger.LogInformation(
                        "Reconnecting after {ReconnectionInterval} due to presumed disconnection",
                        ReconnectionInterval);
                    client.Close();
                    client = new(ConnectionString);
                    client.SubscribeToAnyTopic();
                }
            }
        }
        finally
        {
            client.Close();
            Logger.LogInformation("Stopping");
        }
    }

    internal async Task ProcessMessageAsync(byte[] compressed)
    {
        using IServiceScope serviceScope = ServiceProvider.CreateScope();
        ILogger<EddnMessageHostedService> scopedLogger = ServiceProvider.GetRequiredService<ILogger<EddnMessageHostedService>>();
        IEnumerable<EddnMessageProcessor> messageProcessors = ServiceProvider.GetServices<EddnMessageProcessor>();

        ScopeBuilder scopeBuilder = new ScopeBuilder().Add("UtcTime", DateTime.UtcNow);
        using (scopedLogger.BeginScope(scopeBuilder.Build()))
        {
            try
            {
                string message = Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(compressed));
                JsonDocument jsonDocument = JsonDocument.Parse(message);

                Version? version = GetGameVersion(jsonDocument);
                if (version != null && version >= RequiredGameVersion)
                {
                    foreach (EddnMessageProcessor messageProcessor in messageProcessors)
                    {
                        try
                        {
                            await messageProcessor.ProcessAsync(jsonDocument);
                        }
                        catch (JsonException ex)
                        {
                            scopedLogger.LogError(ex, "Invalid JSON", message);
                        }
                        catch (KeyNotFoundException ex)
                        {
                            scopedLogger.LogError(ex, "Required field(s) missing", message);
                        }
                        catch (FormatException ex)
                        {
                            scopedLogger.LogError(ex, "Incorrect field format", message);
                        }
                        catch (Exception ex)
                        {
                            // Default logger does not log inner exceptions
                            Exception loggedException = ex.InnerException != null ? ex.InnerException : ex;
                            scopedLogger.LogError(loggedException, "Process message failed", message);
                        }
                    }
                }
            }
            catch (ZlibException ex)
            {
                scopedLogger.LogError(ex, "Decompress message failed");
            }
        }
    }

    internal static Version? GetGameVersion(JsonDocument message)
    {
        JsonElement header = message.RootElement.GetProperty("header");
        Version? result = null;
        if (header.TryGetProperty("gameversion", out JsonElement gameVersion))
        {
            Version.TryParse(gameVersion.GetString(), out result);
        }
        return result;
    }
}
