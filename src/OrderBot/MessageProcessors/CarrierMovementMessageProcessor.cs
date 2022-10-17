using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using System.Text.Json;

namespace OrderBot.MessageProcessors
{
    internal class CarrierMovementMessageProcessor : EddnMessageProcessor
    {
        public CarrierMovementMessageProcessor(IDbContextFactory<OrderBotDbContext> contextFactory,
            ILogger<CarrierMovementMessageProcessor> logger, DiscordSocketClient discordClient)
        {
            ContextFactory = contextFactory;
            Logger = logger;
            DiscordClient = discordClient;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
        public ILogger<CarrierMovementMessageProcessor> Logger { get; }
        public DiscordSocketClient DiscordClient { get; }

        public override void Process(string message)
        {
            JsonDocument document = JsonDocument.Parse(message);
            DateTime timestamp = document.RootElement
                    .GetProperty("header")
                    .GetProperty("gatewayTimestamp")
                    .GetDateTime()
                    .ToUniversalTime();

            // See https://github.com/EDCD/EDDN/blob/master/schemas/fsssignaldiscovered-v1.0.json for the schema
            // "signals": [{"IsStation": true, "SignalName": "THE PEAKY BLINDERS KNF-83G", "timestamp": "2022-10-13T12:13:09Z"}]

            JsonElement messageElement = document.RootElement.GetProperty("message");
            if (messageElement.TryGetProperty("event", out JsonElement eventProperty)
                && eventProperty.GetString() == "FSSSignalDiscovered"
                && messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty)
                && messageElement.TryGetProperty("signals", out JsonElement signalsElement))
            {
                Signal[]? signals = signalsElement.Deserialize<Signal[]>();
                if (signals != null)
                {
                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();

                    string starSystemName = starSystemProperty.GetProperty("name").GetString() ?? "";
                    IReadOnlyList<StarSystemCarrier> starSystemCarrier = dbContext.StarSystemCarriers.Include(ssc => ssc.Carrier)
                                                                                                     .Include(ssc => ssc.StarSystem)
                                                                                                     .Where(ss => ss.StarSystem.Name == starSystemName)
                                                                                                     .ToList();
                    IReadOnlyList<DiscordGuild> ignoredCarriers = dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                                                                         .ToList();
                    foreach (Signal signal in signals.Where(s => s.IsStation && Carrier.IsCarrier(s.Name)))
                    {
                        string serialNumber = Carrier.GetSerialNumber(signal.Name);
                        foreach (DiscordGuild discordGuild in
                            ignoredCarriers.Where(dg => !dg.IgnoredCarriers.Any(c => c.SerialNumber == serialNumber)
                                                      && dg.CarrierMovementChannel != null))
                        {
                            StarSystemCarrier? carrierLocation = starSystemCarrier.FirstOrDefault(ssc => ssc.Carrier.SerialNumber == serialNumber);
                            if (carrierLocation == null)
                            {
                                Carrier? carrier = dbContext.Carriers.FirstOrDefault(c => c.SerialNumber == serialNumber);
                                if (carrier == null)
                                {
                                    carrier = new Carrier() { Name = signal.Name };
                                }

                                StarSystem? starSystem = dbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
                                if (starSystem == null)
                                {
                                    starSystem = new StarSystem() { Name = signal.Name, LastUpdated = timestamp };
                                }

                                dbContext.StarSystemCarriers.Add(new StarSystemCarrier() { Carrier = carrier, StarSystem = starSystem, FirstSeen = timestamp });
                                if (DiscordClient.GetChannel(discordGuild.CarrierMovementChannel ?? 0) is ISocketMessageChannel channel)
                                {
                                    try
                                    {
                                        channel.SendMessageAsync(text: $"New carrier '{signal.Name}' seen in '{starSystemName}'");
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.LogError(ex, "Updating channel '{channelId}' for discord Guid '{discordGuildId}' failed", channel.Id, discordGuild.Id);
                                    }
                                }

                                // TODO: Remove old signal sources
                            }
                        }
                    }
                }
            }
        }
    }
}