using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture]
    public class ExtensionsTest
    {
        private const int Delay = 1000, Target = 42;

        [Test]
        public void AwaitDelay()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using var timer = new ScopedTimer(
                watch => Assert.GreaterOrEqual(watch.ElapsedMilliseconds, Delay, "The delay should be properly awaited"),
                loggerFactory);

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
