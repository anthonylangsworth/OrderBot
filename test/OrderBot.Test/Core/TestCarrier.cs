using NUnit.Framework;
using OrderBot.Core;

namespace OrderBot.Test.Core
{
    internal class TestCarrier
    {
        [Test]
        [TestCase("", ExpectedResult = false)]
        [TestCase("a", ExpectedResult = false)]
        [TestCase("ab", ExpectedResult = false)]
        [TestCase("abc-def", ExpectedResult = true)]
        [TestCase("bc-def", ExpectedResult = false)]
        [TestCase("abc-defg", ExpectedResult = false)]
        [TestCase("Ship a1c-de2", ExpectedResult = true)]
        public bool IsCarrier(string signalName)
        {
            return Carrier.IsCarrier(signalName);
        }

        [Test]
        [TestCase("", true)]
        [TestCase("a", true)]
        [TestCase("ab", true)]
        [TestCase("abc-def", false)]
        [TestCase("bc-def", true)]
        [TestCase("abc-defg", true)]
        [TestCase("Ship a1c-de2", false)]
        public void Ctor(string name, bool throws)
        {
            if (throws)
            {
                Assert.That(() => new Carrier() { Name = name }, Throws.ArgumentException);
            }
            else
            {
                Carrier carrier = new Carrier() { Name = name };
                Assert.That(carrier.Name, Is.EqualTo(name));
            }
        }
    }
}
