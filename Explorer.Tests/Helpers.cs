using System.IO;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Explorer.Tests
{
    /// <summary>
    /// Helpers for the explorer testing.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Gets the mock environment with the proper WebRootPath for testing.
        /// </summary>
        /// <returns>The mock environment.</returns>
        public static Mock<IWebHostEnvironment> GetMockEnvironment()
        {
            var repoDir = Directory
                .GetParent(Directory.GetCurrentDirectory())
                .Parent.Parent.Parent.FullName;
            var wwwRoot = Path.Join(repoDir, "Explorer", "wwwroot");

            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment
                .Setup(m => m.ApplicationName)
                .Returns("ComposerTest");
            mockEnvironment
                .Setup(m => m.EnvironmentName)
                .Returns("UnitTestEnvironment");
            mockEnvironment
                .Setup(m => m.WebRootPath)
                .Returns(wwwRoot);
            return mockEnvironment;
        }
    }
}
