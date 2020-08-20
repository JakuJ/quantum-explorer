using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture]
    public class ScopedTimerTest
    {
        [Test]
        public void PerformsTheProvidedAction()
        {
            // Arrange
            var value = "no";

            // Act
            using (new ScopedTimer(() => { value = "yes"; })) { }

            // Assert
            Assert.AreEqual("yes", value, "Scoped timer should perform provided action after disposal");
        }

        [Test]
        public void LogsTheProvidedMessageOnInformationLevel()
        {
            // Arrange
            var logger = new Mock<ILogger<ScopedTimerTest>>();

            // Act
            using (new ScopedTimer("Test message", logger.Object)) { }

            // Assert
            logger.VerifyLevelWasCalled(LogLevel.Information).VerifyNoOtherCalls();
        }
    }
}
