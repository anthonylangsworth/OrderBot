using System.Text.RegularExpressions;

namespace OrderBot.Core
{
    public record Carrier
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
                    throw new ArgumentException($"{value} is a different carrier");
                }
                _name = value;
                SerialNumber = newSerialNumber;
            }
        }

        private readonly static Regex SerialNumberRegex = new("\\w\\w\\w-\\w\\w\\w$");

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
                throw new ArgumentException($"{signalName} is not a valid carrier name");
            }
        }
    }
}