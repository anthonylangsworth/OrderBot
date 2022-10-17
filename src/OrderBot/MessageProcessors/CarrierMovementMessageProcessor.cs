using Microsoft.EntityFrameworkCore;
using OrderBot.Core;
using System.Text.Json;

namespace OrderBot.MessageProcessors
{
    internal class CarrierMovementMessageProcessor : EddnMessageProcessor
    {
        public CarrierMovementMessageProcessor(IDbContextFactory<OrderBotDbContext> contextFactory)
        {
            ContextFactory = contextFactory;
        }

        public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }

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
                    foreach (Signal signal in signals.Where(s => s.IsStation && Carrier.IsCarrier(s.Name)))
                    {
                        foreach (DiscordGuild discordGuild in
                            dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                                   .Where(dg => !dg.IgnoredCarriers.Any(c => c.SerialNumber == Carrier.GetSerialNumber(signal.Name))))
                        {
                            // Debug.WriteLine(starSystemProperty.GetString() + ": " + string.Join("\n", signals.Where(s => s.IsStation && Carrier.IsCarrier(s.SignalName))));
                            // if (dbContext.TrustedCarriers.Any(tc => tc.Carrier)
                        }
                    }
                }
            }
        }
    }
}
