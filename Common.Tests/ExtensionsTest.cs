using System.Collections.Generic;
using System.Linq;
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
        public void EnumerateCollection()
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

        [Test]
        public void TruncateList()
        {
            // Arrange
            List<int> list = Enumerable.Range(0, 10).ToList();

            // Act & Assert
            list.Truncate(10);
            Assert.AreEqual(10, list.Count, "List should not be changed");

            list.Truncate(5);
            Assert.AreEqual(5, list.Count, "List should be truncated to the correct length");
            Assert.IsTrue(list.SequenceEqual(Enumerable.Range(0, 5)), "Correct elements should remain in the list");

            list.Truncate(0);
            Assert.AreEqual(0, list.Count, "List should be empty");
        }
    }
}
