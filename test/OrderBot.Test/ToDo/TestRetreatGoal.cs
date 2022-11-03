using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestRetreatGoal
    {
        [Test]
        public void Instance()
        {
            Assert.That(RetreatGoal.Instance.Name, Is.EqualTo("Retreat"));
            Assert.That(RetreatGoal.Instance.Description, Is.EqualTo("Retreat from the system by reducing influence below 5% and keeping it there."));
            Assert.That(RetreatGoal.InfluenceThreshold, Is.EqualTo(0.05));
        }

        [Test]
        [TestCaseSource(nameof(AddActions_Source))]
        public void AddActions(StarSystem starSystem, double influence, IEnumerable<InfluenceSuggestion> expectedPro, IEnumerable<InfluenceSuggestion> expectedAnti)
        {
            MinorFaction minorFaction = new() { Name = "Flying Fish" };
            StarSystemMinorFaction starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction, Influence = influence };
            ToDoList toDo = new(minorFaction.Name);
            RetreatGoal.Instance.AddSuggestions(starSystemMinorFaction, new HashSet<StarSystemMinorFaction> { starSystemMinorFaction },
                new HashSet<Conflict>(), toDo);
            Assert.That(toDo.Pro, Is.EquivalentTo(expectedPro).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.Anti, Is.EquivalentTo(expectedAnti).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };

            return new[] {
                new TestCaseData(polaris, 0.00, Array.Empty<InfluenceSuggestion>(), new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = 0.0 } }).SetName("AddActions 00"),
                new TestCaseData(polaris, 0.04, Array.Empty<InfluenceSuggestion>(), new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = 0.04 } }).SetName("AddActions 04"),
                new TestCaseData(polaris, 0.05, Array.Empty<InfluenceSuggestion>(), new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = 0.05 } }).SetName("AddActions 05"),
                new TestCaseData(polaris, 0.06, Array.Empty<InfluenceSuggestion>(), new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = 0.06 } }).SetName("AddActions 06")
            };
        }
    }
}
