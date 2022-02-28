using NetMQ;
using NetMQ.Sockets;
using MinorFactionMonitor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Ionic.Zlib;
using System.Text;
using System.Text.Json;

using ServiceProvider serviceProvider = BuildServiceProvider();
EddnMessageProcessor messageProcessor = new EddnMessageProcessor(new[] { "EDA Kunti League" });
Encoding encoding = Encoding.UTF8;
ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();

using (SubscriberSocket client = new SubscriberSocket("tcp://eddn.edcd.io:9500"))
{
    client.SubscribeToAnyTopic();

    while (true)
    {
        if (client.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(1000), out byte[]? compressed, out bool more)
            && compressed != null)
        {
            Task.Factory.StartNew(() => ProcessMessage(messageProcessor, encoding, logger, compressed));
        }
    }
}

ServiceProvider BuildServiceProvider()
{
    ServiceCollection serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    });
    return serviceCollection.BuildServiceProvider();
}

void ProcessMessage(EddnMessageProcessor messageProcessor, Encoding encoding, ILogger logger, byte[] compressed)
{
    using (logger.BeginScope("Process message received at {UtcTime}", DateTime.UtcNow))
    {
        string message = "";
        try
        {
            message = encoding.GetString(ZlibStream.UncompressBuffer(compressed));
            (DateTime timestamp, MinorFactionInfo[] minorFactionDetails) = messageProcessor.GetTimestampAndFactionInfo(message);
        }
        catch (JsonException)
        {
            logger.LogError("Invalid JSON", message);
        }
        catch (KeyNotFoundException)
        {
            logger.LogError("Required field(s) missing", message);
        }
        catch (FormatException)
        {
            logger.LogError("Incorrect field format", message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Process message failed");
        }
    }
}