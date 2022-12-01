namespace OrderBot.Core;
internal class CarrierNameException : Exception
{
    public CarrierNameException(string carrierName)
        : base($"Invalid carrier name {carrierName}")
    {
        CarrierName = carrierName;
    }

    public string CarrierName { get; }
}
