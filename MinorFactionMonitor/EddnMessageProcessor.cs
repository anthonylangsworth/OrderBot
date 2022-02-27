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
        public EddnMessageProcessor(ILogger<EddnMessageProcessor> logger)
        {
            Logger = logger;
        }

        public ILogger<EddnMessageProcessor> Logger { get; }

        public void ProcessMessage(byte[] compressed)
        {
            byte[] uncompressed = ZlibStream.UncompressBuffer(compressed);
            string message = Encoding.UTF8.GetString(uncompressed);
            JsonDocument document;

            try
            {
                document = JsonDocument.Parse(message);
            }
            catch(JsonException ex)
            {
                Logger.LogWarning(ex, "Message is invalid JSON. Ingoring.");
                return;
            }

            DateTime timestamp;
            try
            {
                timestamp = GetTimestamp(document);
            }
            catch (KeyNotFoundException ex)
            {
                Logger.LogWarning(ex, "Timestamp missing. Ingoring.");
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
                && factionsProperty.EnumerateArray().Any(element => "EDA Kunti League".Equals(element.GetProperty("Name").GetString())))
            {
                // TODO: Extract faction information in to MinorFactionInfo[]
               Console.WriteLine(message);
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
