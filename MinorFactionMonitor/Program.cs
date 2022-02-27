using NetMQ;
using NetMQ.Sockets;
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
        if (client.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(1000), out byte[]? compressed, out bool more) 
            && compressed != null)
        {
            Task.Factory.StartNew(() => messageProcessor.ProcessMessage(compressed));
        }
    }
}

static IServiceProvider BuildServiceProvider()
{
    ServiceCollection serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    });
    serviceCollection.AddSingleton(
        serviceProvider => new EddnMessageProcessor(serviceProvider.GetRequiredService<ILogger<EddnMessageProcessor>>(), new [] { "EDA Kunti League"}));
    return serviceCollection.BuildServiceProvider();
}