using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Reports;

namespace OrderBot.Test.Reports
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
        public void AddActions(StarSystem starSystem, double influence, IEnumerable<InfluenceInitiatedAction> expectedPro, IEnumerable<InfluenceInitiatedAction> expectedAnti)
        {
            MinorFaction minorFaction = new() { Name = "Flying Fish" };
            StarSystemMinorFaction starSystemMinorFaction = new StarSystemMinorFaction() { StarSystem = starSystem, MinorFaction = minorFaction, Influence = influence };
            ToDoList toDoList = new(minorFaction.Name);
            ControlGoal.Instance.AddActions(starSystemMinorFaction, toDoList);
            Assert.That(toDoList.Pro, Is.EquivalentTo(expectedPro).Using(DbInfluenceInitiatedActionEqualityComparer.Instance));
            Assert.That(toDoList.Anti, Is.EquivalentTo(expectedAnti).Using(DbInfluenceInitiatedActionEqualityComparer.Instance));
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new StarSystem() { Name = "Polaris", LastUpdated = DateTime.UtcNow };

            return new[] {
                new TestCaseData(polaris, ControlGoal.LowerInfluenceThreshold - 0.01, new [] { new InfluenceInitiatedAction() { StarSystem = polaris, Influence = ControlGoal.LowerInfluenceThreshold - 0.01 } }, Array.Empty<InfluenceInitiatedAction>()).SetName("AddActions Below Lower"),
                new TestCaseData(polaris, ControlGoal.LowerInfluenceThreshold, Array.Empty<InfluenceInitiatedAction>(), Array.Empty<InfluenceInitiatedAction>()).SetName("AddActions Lower"),
                new TestCaseData(polaris, ControlGoal.LowerInfluenceThreshold + 0.01, Array.Empty<InfluenceInitiatedAction>(), Array.Empty<InfluenceInitiatedAction>()).SetName("AddActions Above lower"),
                new TestCaseData(polaris, ControlGoal.UpperInfluenceThreshold - 0.01, Array.Empty<InfluenceInitiatedAction>(), Array.Empty<InfluenceInitiatedAction>()).SetName("AddActions Below Upper"),
                new TestCaseData(polaris, ControlGoal.UpperInfluenceThreshold, Array.Empty<InfluenceInitiatedAction>(), Array.Empty<InfluenceInitiatedAction>()).SetName("AddActions Upper"),
                new TestCaseData(polaris, ControlGoal.UpperInfluenceThreshold + 0.01, Array.Empty<InfluenceInitiatedAction>(), new [] { new InfluenceInitiatedAction() { StarSystem = polaris, Influence = ControlGoal.UpperInfluenceThreshold + 0.01} }).SetName("AddActions Above Upper"),
            };
        }
    }
}
