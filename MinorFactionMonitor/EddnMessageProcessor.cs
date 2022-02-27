using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ionic.Zlib;

namespace MinorFactionMonitor
{
    internal class EddnMessageProcessor
    {
        public EddnMessageProcessor(ILogger<EddnMessageProcessor> logger, IEnumerable<string> minorFactions)
        {
            Logger = logger;
            MinorFactions = new HashSet<string>(minorFactions);

            logger.LogInformation("{Type} started. Monitoring minor factions: {MinorFactions}", GetType().Name, string.Join(",", minorFactions));
        }

        public ILogger<EddnMessageProcessor> Logger { get; }
        public ISet<string> MinorFactions { get; }

        public void ProcessMessage(byte[] compressed)
        {
            try
            {
                JsonDocument document;
                try
                {
                    string message = Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(compressed));
                    document = JsonDocument.Parse(message);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Message is invalid JSON. Ignoring.");
                    return;
                }

                DateTime timestamp;
                try
                {
                    timestamp = GetTimestamp(document);
                }
                catch (KeyNotFoundException ex)
                {
                    Logger.LogWarning(ex, "Timestamp missing. Ignoring.");
                    return;
                }

                JsonElement messageElement = document.RootElement.GetProperty("message");
                string? starSystemName = null;
                if (messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty))
                {
                    starSystemName = starSystemProperty.GetString();
                }
                if (starSystemName != null
                    && messageElement.TryGetProperty("Factions", out JsonElement factionsProperty)
                    && factionsProperty.EnumerateArray().Any(element => MinorFactions.Contains(element.GetProperty("Name").GetString())))
                {
                    // TODO: Extract faction information in to MinorFactionInfo[]
                    Console.WriteLine(document.RootElement.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Unknown error occured processing a message", ex);
            }
        }

        IEnumerable<MinorFactionInfo> FindFactions(Predicate<JsonElement> filter)
        {
            return Enumerable.Empty<MinorFactionInfo>();
        }

        /// <summary>
        /// Extract the UTC timestamp from the messsage.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// The document does not contain a header/gatewayTimestamp element.
        /// </exception>
        DateTime GetTimestamp(JsonDocument document)
        {
            return document.RootElement
                    .GetProperty("header")
                    .GetProperty("gatewayTimestamp")
                    .GetDateTime();
        }
    }
}
