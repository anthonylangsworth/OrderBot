﻿using Ionic.Zlib;
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using SubscriberSocket client = new("tcp://eddn.edcd.io:9500");
        client.SubscribeToAnyTopic();
        Logger.LogInformation("Started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (client.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(1000), out byte[]? compressed, out bool _)
                    && compressed != null)
                {
                    await ProcessMessageAsync(compressed);
                }
            }
        }
        finally
        {
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

                foreach (EddnMessageProcessor messageProcessor in messageProcessors)
                {
                    try
                    {
                        JsonDocument jsonDocument = JsonDocument.Parse(message);
                        Version? version = GetGameVersion(jsonDocument);
                        if (version != null && version >= RequiredGameVersion)
                        {
                            await messageProcessor.ProcessAsync(jsonDocument);
                        }
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
