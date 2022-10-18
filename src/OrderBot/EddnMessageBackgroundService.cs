using Ionic.Zlib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using OrderBot.MessageProcessors;
using System.Text;
using System.Text.Json;

namespace OrderBot
{
    internal class EddnMessageBackgroundService : BackgroundService
    {
        public EddnMessageBackgroundService(ILogger<EddnMessageBackgroundService> logger, IEnumerable<EddnMessageProcessor> messageProcessors)
        {
            MessageProcessors = messageProcessors.ToArray();
            Logger = logger;
        }

        public IReadOnlyList<EddnMessageProcessor> MessageProcessors { get; }
        public ILogger<EddnMessageBackgroundService> Logger { get; }

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
                    await ProcessMessage(Logger, compressed, MessageProcessors);
                }
            }
        }

        internal static async Task ProcessMessage(ILogger logger, byte[] compressed, IReadOnlyList<EddnMessageProcessor> messageProcessors)
        {
            using (logger.BeginScope("Process message received at {UtcTime}", DateTime.UtcNow))
            {
                try
                {
                    string message = Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(compressed));

                    foreach (EddnMessageProcessor messageProcessor in messageProcessors)
                    {
                        try
                        {
                            await Task.Factory.StartNew(() => messageProcessor.Process(JsonDocument.Parse(message)));
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
                catch (ZlibException)
                {
                    logger.LogError("Decompress message failed");
                }
            }
        }
    }
}
