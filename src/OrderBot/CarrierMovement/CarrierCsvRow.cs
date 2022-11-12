using CsvHelper.Configuration.Attributes;

namespace OrderBot.CarrierMovement;

internal record CarrierCsvRow
{
    [Index(0)]
    [Name("Name")]
    public string Name { get; set; } = null!;
}
