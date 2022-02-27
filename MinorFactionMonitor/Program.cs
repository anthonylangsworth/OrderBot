using NetMQ;
using NetMQ.Sockets;
using MinorFactionMonitor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Ionic.Zlib;
using System.Text;

using ServiceProvider serviceProvider = BuildServiceProvider();
EddnMessageProcessor messageProcessor = serviceProvider.GetRequiredService<EddnMessageProcessor>();
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
            Task.Factory.StartNew(() =>
            {
                using (logger.BeginScope("Process message received at {Time}", DateTime.UtcNow))
                {
                    try
                    {
                        string message = encoding.GetString(ZlibStream.UncompressBuffer(compressed));
                        messageProcessor.ProcessMessage(message);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Process message failed");
                    }
                }
            });
        }
    }
}

static ServiceProvider BuildServiceProvider()
{
    ServiceCollection serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    });
    serviceCollection.AddSingleton(
        serviceProvider => new EddnMessageProcessor(
            serviceProvider.GetRequiredService<ILogger<EddnMessageProcessor>>(), 
            new [] { "EDA Kunti League"})
        );
    return serviceCollection.BuildServiceProvider();
}