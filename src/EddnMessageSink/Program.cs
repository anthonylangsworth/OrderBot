using EddnMessageProcessor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using OrderBot.Core;
using System.Text.Json;

using ServiceProvider serviceProvider = BuildServiceProvider();
EddnMessageDecompressor messageDecompressor = new EddnMessageDecompressor();
EddnMessageExtractor messageProcessor = new EddnMessageExtractor(new[] { "EDA Kunti League" });
EddnMessageSink messageSink = new EddnMessageSink(serviceProvider.GetRequiredService<IDbContextFactory<OrderBotDbContext>>());
ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();

using (SubscriberSocket client = new SubscriberSocket("tcp://eddn.edcd.io:9500"))
{
    client.SubscribeToAnyTopic();
    logger.LogInformation("Started");

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
    // Overkill for reading a single environment variable but future proof.
    const string environmentVariablePrefix = "OB__";
    const string databaseEnvironmentVariable = "DB";
    IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddEnvironmentVariables(environmentVariablePrefix)
                                                                     .AddJsonFile("appsettings.json")
                                                                     .Build();
    string dbConnectionString = configurationRoot.GetConnectionString(databaseEnvironmentVariable);
    if (string.IsNullOrEmpty(dbConnectionString))
    {
        throw new InvalidOperationException(
            $"Database connection string missing from environment variable `{environmentVariablePrefix}ConnectionStrings__{databaseEnvironmentVariable}`. " +
            "Usually in the form of `Server=server;Database=OrderBot;User ID=OrderBot;Password=password`.");
    }

    ServiceCollection serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    });
    serviceCollection.AddDbContextFactory<OrderBotDbContext>(
        dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(dbConnectionString)); // "Server=localhost;Database=OrderBot;User ID=OrderBot;Password=password"
                                                                                              // options => options.EnableRetryOnFailure()));
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
            if (starSystem != null && minorFactionDetails.Length > 0)
            {
                messageSink.Sink(timestamp, starSystem, minorFactionDetails);
                logger.LogInformation("System {system} updated", starSystem);
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