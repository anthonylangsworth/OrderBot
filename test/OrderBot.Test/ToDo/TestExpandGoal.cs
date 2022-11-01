using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestExpandGoal
    {
        [Test]
        public void Instance()
        {
            Assert.That(ExpandGoal.Instance.Name, Is.EqualTo("Expand"));
            Assert.That(ExpandGoal.Instance.Description, Is.EqualTo("Expand this minor faction out of this system."));
            Assert.That(ExpandGoal.InfluenceThreshold, Is.EqualTo(0.75));
        }

        [Test]
        [TestCaseSource(nameof(AddActions_Source))]
        public void AddActions(StarSystem starSystem, double influence, string securityLevel,
            IEnumerable<InfluenceInitiatedSuggestion> expectedPro)
        {
            MinorFaction minorFaction = new() { Name = "Flying Fish" };
            StarSystemMinorFaction starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction, Influence = influence, SecurityLevel = securityLevel };
            ToDoList toDo = new(minorFaction.Name);
            ExpandGoal.Instance.AddActions(starSystemMinorFaction, new HashSet<StarSystemMinorFaction> { starSystemMinorFaction }, toDo);
            Assert.That(toDo.Pro, Is.EquivalentTo(expectedPro).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.Anti, Is.Empty);
            Assert.That(toDo.ProSecurity, Is.Empty);
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };

            return new[]
            {
                new TestCaseData(
                    polaris,
                    ExpandGoal.InfluenceThreshold - 0.01,
                    null,
                    new [] { new InfluenceInitiatedSuggestion() { StarSystem = polaris, Influence = ExpandGoal.InfluenceThreshold - 0.01 } }
                ).SetName("AddActions Below InfluenceThreshold"),
                new TestCaseData(
                    polaris,
                    ExpandGoal.InfluenceThreshold,
                    null,
                    Array.Empty<InfluenceInitiatedSuggestion>()
                ).SetName("AddActions InfluenceThreshold"),
                new TestCaseData(
                    polaris,
                    ExpandGoal.InfluenceThreshold + 0.01,
                    null,
                    Array.Empty<InfluenceInitiatedSuggestion>()
                ).SetName("AddActions Above InfluenceThreshold")
            };
        }
    }
}
