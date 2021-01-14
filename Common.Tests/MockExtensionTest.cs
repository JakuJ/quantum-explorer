using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture]
    [Parallelizable]
    public class MockExtensionTest
    {
        [Test]
        public void LoggingLevelVerificationWorks()
        {
            // Arrange
            var logger = new Mock<ILogger<MockExtensionTest>>();

            // Act
            logger.Object.LogDebug("Some message");

            // Assert
            Assert.DoesNotThrow(() => logger.VerifyLevelWasCalled(LogLevel.Debug), "A call to a Debug level log should be detected");
            Assert.Throws<MockException>(() => logger.VerifyLevelWasCalled(LogLevel.Warning), "No other calls should be detected");
        }
    }
}
