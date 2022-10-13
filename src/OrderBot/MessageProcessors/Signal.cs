using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace OrderBot.MessageProcessors
{
    /// <summary>
    /// Based off https://github.com/EDCD/EDDN/blob/master/schemas/fsssignaldiscovered-v1.0.json for the schema
    /// "signals": [{"IsStation": true, "SignalName": "THE PEAKY BLINDERS KNF-83G", "timestamp": "2022-10-13T12:13:09Z"}]
    /// </summary>
    internal record Signal
    {
        [JsonRequired]
        [JsonPropertyName("IsStation")]
        public bool IsStation { get; set; }
        [JsonRequired]
        [JsonPropertyName("SignalName")]
        public string SignalName { get; set; } = null!;
        [JsonRequired]
        [JsonPropertyName("timestamp")]
#pragma warning disable IDE1006
        public DateTime timeStamp { get; set; }
#pragma warning restore IDE1006    
    }
}
