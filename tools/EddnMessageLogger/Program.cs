using Ionic.Zlib;
using NetMQ;
using NetMQ.Sockets;
using System.Text;
using System.Text.Json;

using SubscriberSocket client = new("tcp://eddn.edcd.io:9500");
client.SubscribeToAnyTopic();

Predicate<JsonDocument> messageFilter;
Action<JsonDocument> processMessage; // What to do with a message matching captureMessage

// messageFilter = MentionsFleetCarrierinSystemList;
messageFilter = je => true;
processMessage = SaveMessage;

Console.Out.WriteLine($"Listening for messages");

while (true)
{
    if (client.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(1000), out byte[]? compressed, out bool _)
        && compressed != null)
    {
        string message = "(None)";
        try
        {
            message = Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(compressed));
            JsonDocument jsonDocument = JsonDocument.Parse(message);
            if (messageFilter(jsonDocument))
            {
                processMessage(jsonDocument);
            }
        }
        catch (JsonException)
        {
            Console.Error.WriteLine($"Invalid JSON: {message}");
        }
        catch (KeyNotFoundException)
        {
            Console.Error.WriteLine($"Required field(s) missing: {message}");
        }
        catch (FormatException)
        {
            Console.Error.WriteLine($"Incorrect field format: {message}");
        }
        catch (ZlibException)
        {
            Console.Error.WriteLine("Decompress message failed");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Process message failed {ex}");
        }
    }
}

bool MentionsFleetCarrierinSystemList(JsonDocument jsonDocument)
{
    // See https://github.com/EDCD/EDDN/blob/master/schemas/fsssignaldiscovered-v1.0.json for the schema
    // "signals": [{"IsStation": true, "SignalName": "THE PEAKY BLINDERS KNF-83G", "timestamp": "2022-10-13T12:13:09Z"}]

    string[] systems = new string[]
    {
            "9 G. Carinae",
            "Aha Wa",
            "Anek Wango",
            "Antai",
            "Anukan",
            "Arun",
            "Bhajaja",
            "CD-62 234",
            "CPD-59 314",
            "Eta-1 Pictoris",
            "Gally Bese",
            "Groanomana",
            "HR 1597",
            "HR 2283",
            "Kanates",
            "Kunti",
            "Kutjara",
            "LHS 1832",
            "LHS 199",
            "LPM 229",
            "LTT 2337",
            "LTT 2412",
            "LTT 2684",
            "Luchu",
            "Lutni",
            "Marya Wang",
            "Mors",
            "Naualam",
            "Naunei",
            "Rureri",
            "San Davokje",
            "Sanka",
            "Shambogi",
            "Shongbon",
            "Tabalban",
            "Trumuye",
            "Wuy jugun"
    };

    JsonElement messageElement = jsonDocument.RootElement.GetProperty("message");
    return messageElement.TryGetProperty("event", out JsonElement eventProperty)
        && eventProperty.GetString() == "FSSSignalDiscovered"
        && messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty)
        && systems.Contains(starSystemProperty.GetString(), StringComparer.OrdinalIgnoreCase)
        && messageElement.TryGetProperty("signals", out JsonElement signalsElement);
}

static void SaveMessage(JsonDocument jsonDocument)
{
    DateTime timestamp = jsonDocument.RootElement
            .GetProperty("header")
            .GetProperty("gatewayTimestamp")
            .GetDateTime()
            .ToUniversalTime(); string fileName = $"{timestamp:yyyyMMddTHHmmssFF}.json";
    using FileStream fileStream = File.Create(fileName);
    using Utf8JsonWriter streamWriter = new(fileStream, new JsonWriterOptions() { Indented = true });
    jsonDocument.WriteTo(streamWriter);
    Console.Out.WriteLine($"{fileName} written");
}
