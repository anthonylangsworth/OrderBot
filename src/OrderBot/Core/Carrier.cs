using System.Text.RegularExpressions;

namespace OrderBot.Core
{
    public record Carrier
    {
        private string _name = null!;

        public int Id { get; }

        public string SerialNumber
        {
            get
            {
                return Name.Substring(Name.Length - 7);
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            init
            {
                if (!IsCarrier(value))
                {
                    throw new ArgumentException($"{value} is not a carrier");
                }
                _name = value;
            }
        }

        public static bool IsCarrier(string signalName)
        {
            Regex regex = new("\\w\\w\\w-\\w\\w\\w$");
            return regex.Match(signalName.Trim()).Success;
        }
    }
}