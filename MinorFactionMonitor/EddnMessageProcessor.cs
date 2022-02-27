using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinorFactionMonitor
{
    internal class EddnMessageProcessor
    {
        public EddnMessageProcessor(ILogger<EddnMessageProcessor> logger)
        {
            Logger = logger;
        }

        public ILogger<EddnMessageProcessor> Logger { get; }

        public void ProcessMessage(string result)
        {
            JsonDocument document = JsonDocument.Parse(result);

            try
            {
                DateTime timestamp = GetTimestamp(document);
            }
            catch (KeyNotFoundException ex)
            {
                Logger.LogWarning(ex, "Timestamp missing");
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
               Console.WriteLine(result);
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
