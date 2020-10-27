using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class QsCompilerTest
    {
        [TestCase("Library")]
        [TestCase("MultipleOperations")]
        [TestCase("AllocatedQubitOps")]
        public async Task CompilesExampleCodeWithoutWarnings(string file)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(file);
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            compiler.OnDiagnostics += (sender, s) =>
            {
                Helpers.ConsoleLogger.LogError(s);
                Assert.Fail("There should be no diagnostics emitted");
            };

            // Act
            await compiler.Compile(code);

            // Assert
            Assert.IsNotNull(compiler.Compilation, "Compilation should be successful");
        }

        [Test]
        public async Task ExecutesSampleCode()
        {
            // Arrange
            string sourceCode = await Helpers.GetSourceFile("HelloWorld");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);
            var executed = false;

            compiler.OnDiagnostics += (sender, s) =>
            {
                Helpers.ConsoleLogger.LogError(s);
                Assert.Fail("There should be no diagnostics emitted");
            };

            compiler.OnOutput += (sender, s) =>
            {
                executed = true;
                Assert.AreEqual("Hello World!\n", s, "Intercepted output must be correct.");
            };

            // Act
            await compiler.Compile(sourceCode, true);

            // Assert
            Assert.IsNotNull(compiler.Compilation, "Compilation should be successful");
            Assert.IsTrue(executed, "Execution must emit output.");
        }
    }
}
