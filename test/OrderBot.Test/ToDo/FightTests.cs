using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class FightTests
    {
        [Test]
        [TestCaseSource(nameof(For_Source))]
        public ConflictSuggestion? For(MinorFaction minorFaction, Conflict conflict)
            => Fight.For(minorFaction, conflict);

        public static IEnumerable<TestCaseData> For_Source()
        {
            StarSystem starSystem = new() { Name = "T Tauri" };
            MinorFaction blueSparrows = new() { Name = "Blue Sparrows" };
            MinorFaction redEagles = new() { Name = "Red Eagles" };
            MinorFaction yellowParrots = new() { Name = "Yellow Parrots" };
            Conflict blueVsRed = new()
            {
                MinorFaction1 = blueSparrows,
                MinorFaction1WonDays = 1,
                MinorFaction2 = redEagles,
                MinorFaction2WonDays = 2,
                StarSystem = starSystem,
                Status = ConflictStatus.Active,
                WarType = WarType.Election
            };

            return new TestCaseData[]
            {
                new TestCaseData(blueSparrows, blueVsRed).Returns(new ConflictSuggestion
                {
                    FightFor = blueSparrows,
                    FightForWonDays = blueVsRed.MinorFaction1WonDays,
                    FightAgainst = redEagles,
                    FightAgainstWonDays = blueVsRed.MinorFaction2WonDays,
                    StarSystem = starSystem,
                    State = ConflictState.CloseDefeat,
                    WarType = WarType.Election
                }),
                new TestCaseData(redEagles, blueVsRed).Returns(new ConflictSuggestion
                {
                    FightFor = redEagles,
                    FightForWonDays = blueVsRed.MinorFaction2WonDays,
                    FightAgainst = blueSparrows,
                    FightAgainstWonDays = blueVsRed.MinorFaction1WonDays,
                    StarSystem = starSystem,
                    State = ConflictState.CloseVictory,
                    WarType = WarType.Election
                }),
                new TestCaseData(yellowParrots, blueVsRed).Returns(null)
            };
        }

        [Test]
        [TestCaseSource(nameof(Against_Source))]
        public ConflictSuggestion? Against(MinorFaction minorFaction, Conflict conflict)
            => Fight.Against(minorFaction, conflict);

        public static IEnumerable<TestCaseData> Against_Source()
        {
            StarSystem starSystem = new() { Name = "T Tauri" };
            MinorFaction blueSparrows = new() { Name = "Blue Sparrows" };
            MinorFaction redEagles = new() { Name = "Red Eagles" };
            MinorFaction yellowParrots = new() { Name = "Yellow Parrots" };
            Conflict blueVsRed = new()
            {
                MinorFaction1 = blueSparrows,
                MinorFaction1WonDays = 1,
                MinorFaction2 = redEagles,
                MinorFaction2WonDays = 3,
                StarSystem = starSystem,
                Status = ConflictStatus.Active,
                WarType = WarType.War
            };

            return new TestCaseData[]
            {
                new TestCaseData(blueSparrows, blueVsRed).Returns(new ConflictSuggestion
                {
                    FightFor = redEagles,
                    FightForWonDays = blueVsRed.MinorFaction2WonDays,
                    FightAgainst = blueSparrows,
                    FightAgainstWonDays = blueVsRed.MinorFaction1WonDays,
                    StarSystem = starSystem,
                    State = ConflictState.Victory,
                    WarType = WarType.War
                }),
                new TestCaseData(redEagles, blueVsRed).Returns(new ConflictSuggestion
                {
                    FightFor = blueSparrows,
                    FightForWonDays = blueVsRed.MinorFaction1WonDays,
                    FightAgainst = redEagles,
                    FightAgainstWonDays = blueVsRed.MinorFaction2WonDays,
                    StarSystem = starSystem,
                    State = ConflictState.Defeat,
                    WarType = WarType.War
                }),
                new TestCaseData(yellowParrots, blueVsRed).Returns(null)
            };
        }

        [Test]
        [TestCaseSource(nameof(Between_Source))]
        public ConflictSuggestion? Between(MinorFaction fightFor, MinorFaction fightAgainst, Conflict conflict)
            => Fight.Between(fightFor, fightAgainst, conflict);

        public static IEnumerable<TestCaseData> Between_Source()
        {
            StarSystem starSystem = new() { Name = "T Tauri" };
            MinorFaction blueSparrows = new() { Name = "Blue Sparrows" };
            MinorFaction redEagles = new() { Name = "Red Eagles" };
            MinorFaction yellowParrots = new() { Name = "Yellow Parrots" };
            Conflict blueVsRed = new()
            {
                MinorFaction1 = blueSparrows,
                MinorFaction1WonDays = 1,
                MinorFaction2 = redEagles,
                MinorFaction2WonDays = 2,
                StarSystem = starSystem,
                Status = ConflictStatus.Active,
                WarType = WarType.CivilWar
            };

            return new TestCaseData[]
            {
                new TestCaseData(blueSparrows, redEagles, blueVsRed).Returns(new ConflictSuggestion
                {
                    FightFor = blueSparrows,
                    FightForWonDays = blueVsRed.MinorFaction1WonDays,
                    FightAgainst = redEagles,
                    FightAgainstWonDays = blueVsRed.MinorFaction2WonDays,
                    StarSystem = starSystem,
                    State = ConflictState.CloseDefeat,
                    WarType = WarType.CivilWar
                }),
                new TestCaseData(redEagles, blueSparrows, blueVsRed).Returns(new ConflictSuggestion
                {
                    FightFor = redEagles,
                    FightForWonDays = blueVsRed.MinorFaction2WonDays,
                    FightAgainst = blueSparrows,
                    FightAgainstWonDays = blueVsRed.MinorFaction1WonDays,
                    StarSystem = starSystem,
                    State = ConflictState.CloseVictory,
                    WarType = WarType.CivilWar
                }),
                new TestCaseData(blueSparrows, yellowParrots, blueVsRed).Returns(null),
                new TestCaseData(yellowParrots, blueSparrows, blueVsRed).Returns(null),
                new TestCaseData(yellowParrots, redEagles, blueVsRed).Returns(null),
                new TestCaseData(redEagles, yellowParrots, blueVsRed).Returns(null),
            };
        }
    }
}
