using System.Text.Json.Serialization;

namespace OrderBot.CarrierMovement;

/// <summary>
/// A signal source in the EDDN message schema.
/// </summary>
/// <remarks>
/// <para>
/// Used by <see cref="CarrierMovementMessageProcessor"/> to parse EDDN message JSON.
/// </para>
/// <para>
/// Based off https://github.com/EDCD/EDDN/blob/master/schemas/fsssignaldiscovered-v1.0.json for the schema
/// "signals": [{"IsStation": true, "SignalName": "THE PEAKY BLINDERS KNF-83G", "timestamp": "2022-10-13T12:13:09Z"}]
/// </para>
/// </remarks>
internal record Signal
{
    [JsonInclude]
    [JsonPropertyName("IsStation")]
    public bool IsStation { get; set; }
    [JsonInclude]
    [JsonPropertyName("SignalName")]
    public string Name { get; set; } = null!;
    [JsonInclude]
    [JsonPropertyName("timestamp")]
#pragma warning disable IDE1006
    public DateTime timeStamp { get; set; }
#pragma warning restore IDE1006    
}
