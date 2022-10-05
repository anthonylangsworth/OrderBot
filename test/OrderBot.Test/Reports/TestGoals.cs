using NUnit.Framework;
using OrderBot.Reports;

namespace OrderBot.Test.Reports
{
    internal class TestGoals
    {
        [Test]
        public void Default()
        {
            Assert.That(Goals.Default, Is.EqualTo(ControlGoal.Instance));
        }

        [Test]
        public void Map()
        {
            Assert.That(Goals.Map, Is.EquivalentTo(new Dictionary<string, Goal>
            {
                { ControlGoal.Instance.Name, ControlGoal.Instance },
                { RetreatGoal.Instance.Name, RetreatGoal.Instance }
            }));
        }
    }
}
