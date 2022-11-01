using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestIgnoreGoal
    {
        [Test]
        public void Instance()
        {
            Assert.That(IgnoreGoal.Instance.Name, Is.EqualTo("Ignore"));
            Assert.That(IgnoreGoal.Instance.Description, Is.EqualTo("Never suggested activity."));
        }

        [Test]
        [TestCaseSource(nameof(AddActions_Source))]
        public void AddActions(StarSystem starSystem, double influence, IEnumerable<InfluenceInitiatedSuggestion> expectedPro, IEnumerable<InfluenceInitiatedSuggestion> expectedAnti)
        {
            MinorFaction minorFaction = new() { Name = "Flying Fish" };
            StarSystemMinorFaction starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction, Influence = influence };
            ToDoList toDo = new(minorFaction.Name);
            IgnoreGoal.Instance.AddSuggestions(starSystemMinorFaction, new HashSet<StarSystemMinorFaction> { starSystemMinorFaction },
                new HashSet<Conflict>(), toDo);
            Assert.That(toDo.Pro, Is.EquivalentTo(expectedPro).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.Anti, Is.EquivalentTo(expectedAnti).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };

            return new[] {
                new TestCaseData(polaris, 0.0, Array.Empty<InfluenceInitiatedSuggestion>(), Array.Empty<InfluenceInitiatedSuggestion>()).SetName("AddActions 00"),
                new TestCaseData(polaris, 0.1, Array.Empty<InfluenceInitiatedSuggestion>(), Array.Empty<InfluenceInitiatedSuggestion>()).SetName("AddActions 10"),
                new TestCaseData(polaris, 0.5, Array.Empty<InfluenceInitiatedSuggestion>(), Array.Empty<InfluenceInitiatedSuggestion>()).SetName("AddActions 50"),
                new TestCaseData(polaris, 0.9, Array.Empty<InfluenceInitiatedSuggestion>(), Array.Empty<InfluenceInitiatedSuggestion>()).SetName("AddActions 90")
            };
        }
    }
}
