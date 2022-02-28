using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinorFactionMonitor
{
    /// <summary>
    /// Extract relevant details from EDDN messages.
    /// </summary>
    internal class EddnMessageProcessor
    {
        /// <summary>
        /// Create a new <see cref="EddnMessageProcessor"/>.
        /// </summary>
        /// <param name="minorFactions">
        /// Minor factions to scan for. The names must match in-game names exactly.
        /// </param>
        public EddnMessageProcessor(IEnumerable<string> minorFactions)
        {
            MinorFactions = new HashSet<string>(minorFactions);
        }

        /// <summary>
        /// Minor factions to scan for.
        /// </summary>
        public ISet<string> MinorFactions { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">
        /// The message received from EDDN.
        /// </param>
        /// <returns>
        /// The message's UTC timestamp and an array of <see cref="MinorFactionInfo"/> with relevant
        /// details about the system. If this array is empty, there are no relevant details.
        /// </returns>
        /// <exception cref="JsonException">
        /// The message is not valid JSON.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// The message is valid JSON but is not the expected format.
        /// </exception>
        /// <exception cref="FormatException">
        /// One or more fields are not of the expected format.
        /// </exception>
        public (DateTime, MinorFactionInfo[]) GetTimestampAndFactionInfo(string message)
        {
            JsonDocument document = JsonDocument.Parse(message);
            DateTime timestamp = document.RootElement
                    .GetProperty("header")
                    .GetProperty("gatewayTimestamp")
                    .GetDateTime();

            JsonElement messageElement = document.RootElement.GetProperty("message");
            string? starSystemName = null;
            MinorFactionInfo[] minorFactions = new MinorFactionInfo[0];
            if (messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty))
            {
                starSystemName = starSystemProperty.GetString();
            }
            if (starSystemName != null
                && messageElement.TryGetProperty("Factions", out JsonElement factionsProperty)
                && factionsProperty.EnumerateArray().Any(element => MinorFactions.Contains(element.GetProperty("Name").GetString() ?? "")))
            {
                minorFactions = factionsProperty.EnumerateArray().Select(element =>
                    new MinorFactionInfo(
                        element.GetProperty("Name").GetString() ?? "",
                        element.GetProperty("Influence").GetDouble(),
                        element.TryGetProperty("ActiveStates", out JsonElement activeStatesElement)
                            ? activeStatesElement.EnumerateArray().Select(stateElement => stateElement.GetProperty("State").GetString() ?? "").ToArray()
                            : Array.Empty<string>()
                    )).ToArray();
            }

            return (timestamp, minorFactions);
        }
    }
}
