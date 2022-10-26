using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestMaintainGoal
    {
        [Test]
        public void Instance()
        {
            Assert.That(MaintainGoal.Instance.Name, Is.EqualTo("Maintain"));
            Assert.That(MaintainGoal.Instance.Description, Is.EqualTo("Maintain presence in the system but do not control it."));
            Assert.That(MaintainGoal.LowerInfluenceThreshold, Is.EqualTo(0.1));
            Assert.That(MaintainGoal.MaxInfuenceGap, Is.EqualTo(0.03));
        }

        [Test]
        [TestCaseSource(nameof(AddActions_Source))]
        public void AddActions(StarSystem starSystem, double influence, IEnumerable<InfluenceInitiatedSuggestion> expectedPro, IEnumerable<InfluenceInitiatedSuggestion> expectedAnti)
        {
            MinorFaction minorFaction = new() { Name = "Flying Fish" };
            StarSystemMinorFaction starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction, Influence = influence };
            StarSystemMinorFaction[] systemMinorFactions =
            {
                starSystemMinorFaction,
                new StarSystemMinorFaction() { StarSystem = starSystem, MinorFaction = new (){ Name = "White Whales" }, Influence = 0.2 },
                new StarSystemMinorFaction() { StarSystem = starSystem, MinorFaction = new (){ Name = "Cephalopods" }, Influence = 0.5 }
            };
            ToDoList toDo = new(minorFaction.Name);
            MaintainGoal.Instance.AddActions(starSystemMinorFaction, systemMinorFactions, toDo);
            Assert.That(toDo.Pro, Is.EquivalentTo(expectedPro).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.Anti, Is.EquivalentTo(expectedAnti).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };

            return new[] {
                new TestCaseData(polaris, 0.09, new [] { new InfluenceInitiatedSuggestion() { StarSystem = polaris, Influence = 0.09 } }, Array.Empty<InfluenceInitiatedSuggestion>()).SetName("AddActions Below Lower"),
                new TestCaseData(polaris, 0.1, Array.Empty<InfluenceInitiatedSuggestion>(), Array.Empty<InfluenceInitiatedSuggestion>()).SetName("AddActions Lower"),
                new TestCaseData(polaris, 0.15, Array.Empty<InfluenceInitiatedSuggestion>(), Array.Empty<InfluenceInitiatedSuggestion>()).SetName("AddActions Above lower"),
                new TestCaseData(polaris, 0.46, Array.Empty<InfluenceInitiatedSuggestion>(), Array.Empty<InfluenceInitiatedSuggestion>()).SetName("AddActions Below Upper"),
                new TestCaseData(polaris, 0.47, Array.Empty<InfluenceInitiatedSuggestion>(), Array.Empty<InfluenceInitiatedSuggestion>()).SetName("AddActions Upper"),
                new TestCaseData(polaris, 0.48, Array.Empty<InfluenceInitiatedSuggestion>(), new [] { new InfluenceInitiatedSuggestion() { StarSystem = polaris, Influence = 0.48 } }).SetName("AddActions Above Upper"),
            };
        }
    }
}
