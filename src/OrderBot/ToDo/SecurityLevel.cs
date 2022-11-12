namespace OrderBot.ToDo;

internal static class SecurityLevel
{
    public static string Low => "$SYSTEM_SECURITY_low;";
    public static string Medium => "$SYSTEM_SECURITY_medium;";
    public static string High => "$SYSTEM_SECURITY_high;";

    public static IDictionary<string, string> Name =>
        new Dictionary<string, string>()
        {
            { Low, "Low"},
            { Medium, "Medium"},
            { High, "High"}
        };
}
