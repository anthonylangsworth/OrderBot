using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestControlGoal
    {
        [Test]
        public void Instance()
        {
            Assert.That(ControlGoal.Instance.Name, Is.EqualTo("Control"));
            Assert.That(ControlGoal.Instance.Description, Is.EqualTo("Be the minor faction with the highest influence. Keep influence between 55% and 65%."));
            Assert.That(ControlGoal.LowerInfluenceThreshold, Is.EqualTo(0.55));
            Assert.That(ControlGoal.UpperInfluenceThreshold, Is.EqualTo(0.65));
        }

        [Test]
        [TestCaseSource(nameof(AddActions_Source))]
        public void AddActions(StarSystem starSystem, double influence, string securityLevel,
            IEnumerable<InfluenceInitiatedSuggestion> expectedPro,
            IEnumerable<InfluenceInitiatedSuggestion> expectedAnti,
            IEnumerable<SecurityInitiatedSuggestion> expectedProSecurity)
        {
            MinorFaction minorFaction = new() { Name = "Flying Fish" };
            StarSystemMinorFaction starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction, Influence = influence, SecurityLevel = securityLevel };
            ToDoList toDo = new(minorFaction.Name);
            ControlGoal.Instance.AddActions(starSystemMinorFaction, new HashSet<StarSystemMinorFaction> { starSystemMinorFaction }, toDo);
            Assert.That(toDo.Pro, Is.EquivalentTo(expectedPro).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.Anti, Is.EquivalentTo(expectedAnti).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.ProSecurity, Is.EquivalentTo(expectedProSecurity).Using(DbSecurityInitiatedSuggestionEqualityComparer.Instance));
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };

            return new[]
            {
                new TestCaseData(
                    polaris,
                    ControlGoal.LowerInfluenceThreshold - 0.01,
                    null,
                    new [] { new InfluenceInitiatedSuggestion() { StarSystem = polaris, Influence = ControlGoal.LowerInfluenceThreshold - 0.01 } },
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    Array.Empty<SecurityInitiatedSuggestion>()
                ).SetName("AddActions Below Lower"),
                new TestCaseData(
                    polaris,
                    ControlGoal.LowerInfluenceThreshold,
                    null,
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    Array.Empty<SecurityInitiatedSuggestion>()
                ).SetName("AddActions Lower"),
                new TestCaseData(
                    polaris,
                    ControlGoal.LowerInfluenceThreshold + 0.01,
                    null,
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    Array.Empty<SecurityInitiatedSuggestion>()
                ).SetName("AddActions Above lower"),
                new TestCaseData(
                    polaris,
                    ControlGoal.UpperInfluenceThreshold - 0.01,
                    SecurityLevel.High,
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    Array.Empty<SecurityInitiatedSuggestion>()
                ).SetName("AddActions Below Upper"),
                new TestCaseData(
                    polaris,
                    ControlGoal.UpperInfluenceThreshold,
                    SecurityLevel.Medium,
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    Array.Empty<SecurityInitiatedSuggestion>()
                ).SetName("AddActions Upper"),
                new TestCaseData(
                    polaris,
                    ControlGoal.UpperInfluenceThreshold + 0.01,
                    SecurityLevel.Low,
                    Array.Empty<InfluenceInitiatedSuggestion>(),
                    new [] { new InfluenceInitiatedSuggestion() { StarSystem = polaris, Influence = ControlGoal.UpperInfluenceThreshold + 0.01} },
                    new [] { new SecurityInitiatedSuggestion() { StarSystem = polaris, SecurityLevel = SecurityLevel.Low} }
                ).SetName("AddActions Above Upper"),
            };
        }
    }
}
