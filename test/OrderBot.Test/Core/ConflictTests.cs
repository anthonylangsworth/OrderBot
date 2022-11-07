using NUnit.Framework;
using OrderBot.Core;

namespace OrderBot.Test.Core
{
    internal class ConflictTests
    {
        [Test]
        [TestCaseSource(nameof(GetState_Source))]
        public string GetState(string status, int fightForWonDays, int fightAgainstWonDays)
        {
            return Conflict.GetState(status, fightForWonDays, fightAgainstWonDays);
        }

        public static IEnumerable<TestCaseData> GetState_Source()
        {
            return new TestCaseData[]
            {
                new TestCaseData(ConflictStatus.Active, 0, 4).Returns(ConflictState.TotalDefeat),
                new TestCaseData(ConflictStatus.Active, 0, 3).Returns(ConflictState.TotalDefeat),
                new TestCaseData(ConflictStatus.Active, 0, 2).Returns(ConflictState.Defeat),
                new TestCaseData(ConflictStatus.Active, 0, 1).Returns(ConflictState.CloseDefeat),
                new TestCaseData(ConflictStatus.Active, 0, 0).Returns(ConflictState.Draw),
                new TestCaseData(ConflictStatus.Active, 1, 0).Returns(ConflictState.CloseVictory),
                new TestCaseData(ConflictStatus.Active, 2, 0).Returns(ConflictState.Victory),
                new TestCaseData(ConflictStatus.Active, 3, 0).Returns(ConflictState.TotalVictory),
                new TestCaseData(ConflictStatus.Active, 4, 0).Returns(ConflictState.TotalVictory),
                // Increment won days to ensure subtraction not lookup
                new TestCaseData(ConflictStatus.Active, 1, 5).Returns(ConflictState.TotalDefeat),
                new TestCaseData(ConflictStatus.Active, 1, 4).Returns(ConflictState.TotalDefeat),
                new TestCaseData(ConflictStatus.Active, 1, 3).Returns(ConflictState.Defeat),
                new TestCaseData(ConflictStatus.Active, 1, 2).Returns(ConflictState.CloseDefeat),
                new TestCaseData(ConflictStatus.Active, 1, 1).Returns(ConflictState.Draw),
                new TestCaseData(ConflictStatus.Active, 2, 1).Returns(ConflictState.CloseVictory),
                new TestCaseData(ConflictStatus.Active, 3, 1).Returns(ConflictState.Victory),
                new TestCaseData(ConflictStatus.Active, 4, 1).Returns(ConflictState.TotalVictory),
                new TestCaseData(ConflictStatus.Active, 5, 1).Returns(ConflictState.TotalVictory),

                new TestCaseData("Not Active", 0, 0).Returns("Not Active")
            };
        }

        [Test]
        [TestCaseSource(nameof(IsWar_Source))]
        public bool IsWar(string warType)
        {
            return Conflict.IsWar(warType);
        }

        public static IEnumerable<TestCaseData> IsWar_Source()
        {
            return new TestCaseData[]
            {
                new TestCaseData(WarType.War).Returns(true),
                new TestCaseData(WarType.CivilWar).Returns(true),
                new TestCaseData(WarType.Election).Returns(false)
            };
        }

        [Test]
        [TestCaseSource(nameof(IsElection_Source))]
        public bool IsElection(string warType)
        {
            return Conflict.IsElection(warType);
        }

        public static IEnumerable<TestCaseData> IsElection_Source()
        {
            return new TestCaseData[]
            {
                new TestCaseData(WarType.War).Returns(false),
                new TestCaseData(WarType.CivilWar).Returns(false),
                new TestCaseData(WarType.Election).Returns(true)
            };
        }
    }
}
