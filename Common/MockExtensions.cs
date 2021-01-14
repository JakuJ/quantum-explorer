using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace Common
{
    /// <summary>
    /// Extension methods that simplify complex <see cref="Mock"/> setups or verifications.
    /// </summary>
    public static class MockExtensions
    {
        /// <summary>
        /// Verifies that a mocked <see cref="ILogger{TCategoryName}"/> attempted to log at a certain level.
        /// </summary>
        /// <param name="logger">The mock logger.</param>
        /// <param name="level">The level of logging to check.</param>
        /// <typeparam name="T">The category of the logger.</typeparam>
        /// <returns>The input mock logger.</returns>
        public static Mock<ILogger<T>> VerifyLevelWasCalled<T>(this Mock<ILogger<T>> logger, LogLevel level)
        {
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == level),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return logger;
        }
    }
}
