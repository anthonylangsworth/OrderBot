namespace OrderBot.Core
{
    public class Conflict
    {
        public int Id { get; init; }
        public StarSystem StarSystem { get; init; } = null!;
        public MinorFaction MinorFaction1 { get; init; } = null!;
        public int MinorFaction1WonDays { get; set; } = 0;
        public MinorFaction MinorFaction2 { get; init; } = null!;
        public int MinorFaction2WonDays { get; set; } = 0;
        public string? Status { get; set; } = null;
        public string WarType { get; init; } = null!;

        public static string GetState(string status, int fightForWonDays, int fightAgainstWonDays)
        {
            string result;
            if (string.IsNullOrWhiteSpace(status) || status == ConflictStatus.Active)
            {
                result = (fightForWonDays - fightAgainstWonDays) switch
                {
                    <= -3 => ConflictState.TotalDefeat,
                    -2 => ConflictState.Defeat,
                    -1 => ConflictState.CloseDefeat,
                    0 => ConflictState.Draw,
                    1 => ConflictState.CloseVictory,
                    2 => ConflictState.Victory,
                    >= 3 => ConflictState.TotalVictory
                };
            }
            else
            {
                result = status;
            }
            return result;
        }

        public static bool IsWar(string warType)
        {
            return warType == Core.WarType.War || warType == Core.WarType.CivilWar;
        }

        public static bool IsElection(string warType)
        {
            return warType == Core.WarType.Election;
        }

    }
}
