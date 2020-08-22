using System.Threading.Tasks;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture]
    public class ExtensionsTest
    {
        private const int Delay = 1000, DelayError = 50, Target = 42;

        [Test]
        public void AwaitDelay()
        {
            using var timer =
                new ScopedTimer(watch => Assert.GreaterOrEqual(
                                    watch.ElapsedMilliseconds, Delay - DelayError,
                                    "The delay should be properly awaited"));

            JustWait().WaitAndUnwrapException();
        }

        [Test]
        public void AwaitAnAsyncValue()
        {
            int value = WaitAndReturn().WaitAndUnwrapException();
            Assert.AreEqual(Target, value, "The awaited value should be correct");
        }

        private static async Task JustWait() => await Task.Delay(Delay);

        private static async Task<int> WaitAndReturn()
        {
            await Task.Delay(Delay);
            return Target;
        }
    }
}
