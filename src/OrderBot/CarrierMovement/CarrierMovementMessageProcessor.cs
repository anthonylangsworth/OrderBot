using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;
using System.Net;
using System.Text.Json;
using System.Transactions;

namespace OrderBot.CarrierMovement
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

        public override void Process(JsonDocument message)
        {
            DateTime timestamp = message.RootElement
                    .GetProperty("header")
                    .GetProperty("gatewayTimestamp")
                    .GetDateTime()
                    .ToUniversalTime();

            // See https://github.com/EDCD/EDDN/blob/master/schemas/fsssignaldiscovered-v1.0.json for the schema
            // "signals": [{"IsStation": true, "SignalName": "THE PEAKY BLINDERS KNF-83G", "timestamp": "2022-10-13T12:13:09Z"}]

            JsonElement messageElement = message.RootElement.GetProperty("message");
            if (messageElement.TryGetProperty("event", out JsonElement eventProperty)
                && eventProperty.GetString() == "FSSSignalDiscovered"
                && messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty)
                && messageElement.TryGetProperty("signals", out JsonElement signalsElement))
            {
                Signal[]? signals = signalsElement.Deserialize<Signal[]>();
                if (signals != null)
                {
                    using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
                    string starSystemName = starSystemProperty.GetString() ?? "";
                    StarSystem? starSystem = dbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
                    if (starSystem != null)
                    {
                        using TransactionScope transactionScope = new();
                        IReadOnlyList<DiscordGuild> discordGuilds = dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                                                                           .Where(dg => dg.CarrierMovementChannel != null)
                                                                                           .ToList();
                        Carrier[] observedCarriers = UpdateNewCarrierLocations(dbContext, starSystem, discordGuilds, timestamp, signals);
                        // Not all messages are complete. Therefore, we cannot say a carrier has jumped out
                        // if we do not receive a signal for it.
                        // RemoveAbsentCarrierLocations(dbContext, starSystem, discordGuilds, observedCarriers);
                        transactionScope.Complete();
                    }
                }
            }
        }

        internal Carrier[] UpdateNewCarrierLocations(OrderBotDbContext dbContext, StarSystem starSystem, IReadOnlyList<DiscordGuild> discordGuilds, DateTime timestamp, Signal[] signals)
        {
            List<Carrier> observedCarriers = new();
            foreach (Signal signal in signals.Where(s => s.IsStation && Carrier.IsCarrier(s.Name)))
            {
                string serialNumber = Carrier.GetSerialNumber(signal.Name);
                Carrier? carrier = dbContext.Carriers.Include(c => c.StarSystem)
                                                     .FirstOrDefault(c => c.SerialNumber == serialNumber);
                if (carrier == null)
                {
                    carrier = new Carrier() { Name = signal.Name };
                    dbContext.Carriers.Add(carrier);
                }
                observedCarriers.Add(carrier);
                if (carrier.StarSystem != starSystem)
                {
                    carrier.StarSystem = starSystem;
                    carrier.FirstSeen = timestamp;

                    foreach (DiscordGuild discordGuild in
                        discordGuilds.Where(dg => !dg.IgnoredCarriers.Any(c => c.SerialNumber == serialNumber)))
                    {
                        if (DiscordClient.GetChannel(discordGuild.CarrierMovementChannel ?? 0) is ISocketMessageChannel channel)
                        {
                            try
                            {
                                channel.SendMessageAsync(
                                        text: $"New fleet carrier '{signal.Name}'(<https://inara.cz/elite/search/?search={WebUtility.UrlEncode(serialNumber)}>) seen in '{starSystem.Name}'(<https://inara.cz/elite/search/?search={WebUtility.UrlEncode(starSystem.Name)}>)."
                                    )
                                    .GetAwaiter().GetResult();
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(
                                    ex,
                                    "Sending carrier notification to channel '{channelId}' for discord Guid '{discordGuildId}' failed",
                                    channel.Id,
                                    discordGuild.Id
                                );
                            }
                        }
                    }
                }
            }
            dbContext.SaveChanges();
            return observedCarriers.ToArray();
        }

        // Not all messages are complete. Therefore, we cannot say a carrier has jumped out
        // if we do not receive a signal for it.
        //private void RemoveAbsentCarrierLocations(OrderBotDbContext dbContext, StarSystem starSystem, IReadOnlyList<DiscordGuild> discordGuilds, Carrier[] observedCarriers)
        //{
        //    foreach (Carrier carrier in dbContext.Carriers.Where(c => c.StarSystem == starSystem && !observedCarriers.Contains(c)))
        //    {
        //        carrier.StarSystem = null;
        //        carrier.FirstSeen = null;

        //        foreach (DiscordGuild discordGuild in
        //            discordGuilds.Where(dg => !dg.IgnoredCarriers.Any(c => c.SerialNumber == carrier.SerialNumber)))
        //        {
        //            if (DiscordClient.GetChannel(discordGuild.CarrierMovementChannel ?? 0) is ISocketMessageChannel channel)
        //            {
        //                try
        //                {
        //                    channel.SendMessageAsync(text: $"Fleet carrier '{carrier.Name}' has left '{starSystem.Name}'. Inara: https://inara.cz/elite/search/?search={carrier.SerialNumber}").GetAwaiter().GetResult();
        //                }
        //                catch (Exception ex)
        //                {
        //                    Logger.LogError(ex, "Updating channel '{channelId}' for discord Guid '{discordGuildId}' failed", channel.Id, discordGuild.Id);
        //                }
        //            }
        //        }
        //    }
        //    dbContext.SaveChanges();
        //}
    }
}
