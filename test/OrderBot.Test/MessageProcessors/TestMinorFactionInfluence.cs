using NUnit.Framework;
using OrderBot.MessageProcessors;

namespace OrderBot.Test.MessageProcessors
{
    internal class TestMinorFactionInfluence
    {
        [TestCaseSource(nameof(Ctor_Source))]
        public void Ctor(string minorFaction, double influence, IEnumerable<string> states)
        {
            MinorFactionInfluence minorFactionInfluence = new MinorFactionInfluence(minorFaction, influence, states);
            Assert.That(minorFactionInfluence.MinorFaction, Is.EqualTo(minorFaction));
            Assert.That(minorFactionInfluence.Influence, Is.EqualTo(influence));
            Assert.That(minorFactionInfluence.States, Is.EquivalentTo(states));
        }

        public static IEnumerable<TestCaseData> Ctor_Source()
        {
            return new[]
            {
                new TestCaseData("a", 0.1, Array.Empty<string>()),
                new TestCaseData("b", 0.8, new string[] { "Boom", "Terrorist Attack" })
            };
        }
    }
}
