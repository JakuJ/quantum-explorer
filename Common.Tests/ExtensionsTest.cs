using System.Threading.Tasks;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture]
    [Parallelizable]
    public class ExtensionsTest
    {
        private const int Delay = 1000, DelayError = 50, Target = 42;

        [Test]
        public void AwaitDelay()
        {
            // Arrange
            static async Task JustWait() => await Task.Delay(Delay);

            // Act & Assert
            using var timer =
                new ScopedTimer(watch => Assert.GreaterOrEqual(
                                    watch.ElapsedMilliseconds,
                                    Delay - DelayError,
                                    "The delay should be properly awaited"));

            JustWait().WaitAndUnwrapException();
        }

        [Test]
        public void AwaitAnAsyncValue()
        {
            // Arrange
            static async Task<int> WaitAndReturn()
            {
                await Task.Delay(Delay);
                return Target;
            }

            // Act
            int value = WaitAndReturn().WaitAndUnwrapException();

            // Assert
            Assert.AreEqual(Target, value, "The awaited value should be correct");
        }

        [Test]
        public void EnumerateACollection()
        {
            // Arrange
            string[] collection = { "Zero", "One", "Two", "Three" };

            // Act & Assert
            var index = 0;
            foreach ((int ix, string value) in collection.Enumerate())
            {
                Assert.AreEqual(index, ix, "Enumerated indices should be correct.");
                Assert.AreEqual(value, collection[index], "Values should be enumerated in the right order.");
                index++;
            }
        }
    }
}
