using CsvHelper.Configuration.Attributes;

namespace OrderBot.ToDo
{
    internal record GoalCsvRow
    {
        [Index(0)]
        [Name("Goal")]
        public string Goal { get; set; } = null!;
        [Index(1)]
        [Name("Minor Faction")]
        public string MinorFaction { get; set; } = null!;
        [Index(2)]
        [Name("Star System")]
        public string StarSystem { get; set; } = null!;
    }
}
