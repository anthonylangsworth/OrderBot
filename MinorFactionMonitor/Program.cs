using Ionic.Zlib;
using NetMQ;
using NetMQ.Sockets;
using System.Text;
using MinorFactionMonitor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

IServiceProvider serviceProvider = BuildServiceProvider();
EddnMessageProcessor messageProcessor = serviceProvider.GetRequiredService<EddnMessageProcessor>();

using (SubscriberSocket client = new SubscriberSocket())
{
    client.Connect("tcp://eddn.edcd.io:9500");
    client.SubscribeToAnyTopic();

    while (true)
    {
        if (client.TryReceiveFrameBytes(out byte[]? compressed, out bool more))
        {
            if (compressed != null)
            {
                byte[] uncompressed = ZlibStream.UncompressBuffer(compressed);
                string message = Encoding.UTF8.GetString(uncompressed);
                Task.Factory.StartNew(() => messageProcessor.ProcessMessage(message));
            }
        }
    }
}

static IServiceProvider BuildServiceProvider()
{
    ServiceCollection serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging((logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    }));
    serviceCollection.AddSingleton<EddnMessageProcessor>();
    return serviceCollection.BuildServiceProvider();
}