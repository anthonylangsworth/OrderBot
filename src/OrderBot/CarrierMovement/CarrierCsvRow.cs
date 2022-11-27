using CsvHelper.Configuration.Attributes;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Exports and imports carrier data in <see cref="CarrierMovementCommandsModule"/>.
/// </summary>
internal record CarrierCsvRow
{
    [Index(0)]
    [Name("Name")]
    public string Name { get; set; } = null!;
}
