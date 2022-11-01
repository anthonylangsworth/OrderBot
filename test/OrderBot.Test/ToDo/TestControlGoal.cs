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
            IEnumerable<InfluenceSuggestion> expectedPro,
            IEnumerable<InfluenceSuggestion> expectedAnti,
            IEnumerable<SecuritySuggestion> expectedProSecurity)
        {
            MinorFaction minorFaction = new() { Name = "Flying Fish" };
            StarSystemMinorFaction starSystemMinorFaction = new() { StarSystem = starSystem, MinorFaction = minorFaction, Influence = influence, SecurityLevel = securityLevel };
            ToDoList toDo = new(minorFaction.Name);
            ControlGoal.Instance.AddSuggestions(starSystemMinorFaction, new HashSet<StarSystemMinorFaction> { starSystemMinorFaction },
                new HashSet<Conflict>(), toDo);
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
                    new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = ControlGoal.LowerInfluenceThreshold - 0.01 } },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>()
                ).SetName("AddActions Below Lower"),
                new TestCaseData(
                    polaris,
                    ControlGoal.LowerInfluenceThreshold,
                    null,
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>()
                ).SetName("AddActions Lower"),
                new TestCaseData(
                    polaris,
                    ControlGoal.LowerInfluenceThreshold + 0.01,
                    null,
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>()
                ).SetName("AddActions Above lower"),
                new TestCaseData(
                    polaris,
                    ControlGoal.UpperInfluenceThreshold - 0.01,
                    SecurityLevel.High,
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>()
                ).SetName("AddActions Below Upper"),
                new TestCaseData(
                    polaris,
                    ControlGoal.UpperInfluenceThreshold,
                    SecurityLevel.Medium,
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>()
                ).SetName("AddActions Upper"),
                new TestCaseData(
                    polaris,
                    ControlGoal.UpperInfluenceThreshold + 0.01,
                    SecurityLevel.Low,
                    Array.Empty<InfluenceSuggestion>(),
                    new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = ControlGoal.UpperInfluenceThreshold + 0.01} },
                    new [] { new SecuritySuggestion() { StarSystem = polaris, SecurityLevel = SecurityLevel.Low} }
                ).SetName("AddActions Above Upper"),
            };
        }
    }
}
