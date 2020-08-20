using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture]
    public class ScopedTimerTest
    {
        [Test]
        public void PerformsTheProvidedAction()
        {
            var value = "no";
            using (new ScopedTimer(() => { value = "yes"; })) { }

            Assert.AreEqual("yes", value, "Scoped timer should perform provided action after disposal");
        }

        // TODO: A test for the logging constructor
    }
}
