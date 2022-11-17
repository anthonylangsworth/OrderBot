using System.ComponentModel.DataAnnotations;

namespace OrderBot.Discord;

public class DiscordClientOptions
{
    /// <summary>
    /// Discord API Key.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = null!;
}
