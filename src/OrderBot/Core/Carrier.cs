using System.Text.RegularExpressions;

namespace OrderBot.Core;

public partial class Carrier
{
    private string _name = null!;

    public int Id { get; }

    public ICollection<DiscordGuild> IgnoredBy { get; init; } = null!;

    public string? Owner { get; set; } = null!;

    public StarSystem? StarSystem { get; set; } = null!;

    public DateTime? FirstSeen { get; set; } = null!;

    public string SerialNumber { get; private set; } = null!;

    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            string newSerialNumber = GetSerialNumber(value);
            if (SerialNumber != null && newSerialNumber != SerialNumber)
            {
                throw new CarrierNameException(value);
            }
            _name = value;
            SerialNumber = newSerialNumber;
        }
    }

    private readonly static Regex SerialNumberRegex = MyRegex();

    public static bool IsCarrier(string signalName)
    {
        return SerialNumberRegex.Match(signalName.Trim()).Success;
    }

    public static string GetSerialNumber(string signalName)
    {
        Match match = SerialNumberRegex.Match(signalName);
        if (match.Success)
        {
            return match.Value;
        }
        else
        {
            throw new CarrierNameException(signalName);
        }
    }

    public override string ToString()
    {
        return $"{Name} in {StarSystem?.Name ?? "(None)"} at {FirstSeen}";
    }

    [GeneratedRegex("\\w\\w\\w-\\w\\w\\w$")]
    private static partial Regex MyRegex();
}
