using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class QsCompilerTest
    {
        private static readonly ILogger<QsCompiler> ConsoleLogger
            = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<QsCompiler>();

        [Test]
        public async Task CompilesExampleCodeWithoutWarnings()
        {
            // Arrange
            string code = await GetSource("Library");
            using var compiler = new QsCompiler(ConsoleLogger);

            // Act
            await compiler.Compile(code);

            // Assert
            Assert.AreEqual(0, compiler.GetDiagnostics().Count, "There should be no warnings or errors");
        }

        private static async Task<string> GetSource(string baseName)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $"TestSources/{baseName}.qs");
            return await File.ReadAllTextAsync(path);
        }
    }
}
