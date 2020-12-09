using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class QsCompilerTest
    {
        private const string NoEntryPointMessage = "Nothing to execute, no entry point specified.";

        [TestCase("Library")]
        [TestCase("MultipleOperations")]
        [TestCase("AllocatedQubitOps")]
        public async Task CompilesExampleCodeWithoutWarnings(string file)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(file);
            QsCompiler compiler = new(Helpers.ConsoleLogger);
            var compiled = false;

            compiler.OnDiagnostics += (_, s) =>
            {
                Helpers.ConsoleLogger.LogError(s);
                if (s != NoEntryPointMessage)
                {
                    Assert.Fail($"No diagnostics should be present! Got: {s}");
                }
            };

            compiler.OnGrids += (_, _) => { compiled = true; };

            // Act
            await compiler.Compile(code);

            // Assert
            Assert.IsTrue(compiled, "Compilation should be successful");
        }

        [Test]
        public async Task CompilesAndRunsWithEntryPoint()
        {
            // Arrange
            string sourceCode = await Helpers.GetSourceFile("HelloWorld");
            QsCompiler compiler = new(Helpers.ConsoleLogger);
            var compiled = false;
            var executed = false;

            compiler.OnDiagnostics += (_, s) => { Assert.Fail($"There should be no diagnostics emitted! Got: {s}"); };

            compiler.OnOutput += (_, s) =>
            {
                executed = true;
                Assert.AreEqual("Hello World!\n", s, "Intercepted output must be correct.");
            };

            compiler.OnGrids += (_, _) => { compiled = true; };

            // Act
            await compiler.Compile(sourceCode);

            // Assert
            Assert.IsTrue(compiled, "Compilation should be successful");
            Assert.IsTrue(executed, "Execution must emit output.");
        }
    }
}
