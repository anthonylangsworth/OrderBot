using NetMQ;
using NetMQ.Sockets;
using EddnMessageSink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using OrderBot.Core;

using ServiceProvider serviceProvider = BuildServiceProvider();
EddnMessageDecompressor messageDecompressor = new EddnMessageDecompressor();
EddnMessageExtractor messageProcessor = new EddnMessageExtractor(new[] { "EDA Kunti League" });
ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();

using (SubscriberSocket client = new SubscriberSocket("tcp://eddn.edcd.io:9500"))
{
    client.SubscribeToAnyTopic();

    while (true)
    {
        if (client.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(1000), out byte[]? compressed, out bool more)
            && compressed != null)
        {
            Task.Factory.StartNew(() => ProcessMessage(messageDecompressor, messageProcessor, logger, compressed));
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

void ProcessMessage(EddnMessageDecompressor messageDecompressor, EddnMessageExtractor messageProcessor, ILogger logger, byte[] compressed)
{
    using (logger.BeginScope("Process message received at {UtcTime}", DateTime.UtcNow))
    {
        string message = "";
        try
        {
            message = messageDecompressor.Decompress(compressed);
            (DateTime timestamp, string? starSystem, MinorFactionInfo[] minorFactionDetails) = messageProcessor.GetTimestampAndFactionInfo(message);

            if (starSystem != null)
            {
                using (OrderBotDbContext dbContext = new OrderBotDbContext())
                {
                    foreach (MinorFactionInfo newMinorFactionInfo in minorFactionDetails)
                    {
                        SystemMinorFaction? existingSystemMinorFaction = dbContext.SystemMinorFaction
                                                                                  .FirstOrDefault(smf => smf.StarSystem == starSystem && smf.MinorFaction == newMinorFactionInfo.minorFaction);
                        if(existingSystemMinorFaction == null)
                        {
                            dbContext.SystemMinorFaction.Add(new SystemMinorFaction
                            {
                                MinorFaction = newMinorFactionInfo.minorFaction,
                                StarSystem = starSystem,
                                Influence = newMinorFactionInfo.influence,
                                Goal = Goals.Default.Name,
                                LastUpdated = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            existingSystemMinorFaction.Influence = newMinorFactionInfo.influence;
                            existingSystemMinorFaction.LastUpdated = DateTime.UtcNow;
                        }
                    }
                    dbContext.SaveChanges();
                }
            }
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