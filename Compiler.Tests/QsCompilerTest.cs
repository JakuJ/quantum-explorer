using System.Threading.Tasks;
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

            compiler.OnDiagnostics += (sender, s) => Assert.Fail("There should be no diagnostics emitted");

            // Act
            await compiler.Compile(code);

            // Assert
            Assert.IsNotNull(compiler.Compilation, "Compilation should be successful");
        }

        [Test]
        public async Task ExecutesSampleCode()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("HelloWorld");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);
            var executed = false;

            compiler.OnDiagnostics += (sender, s) => Assert.Fail($"There should be no diagnostics emitted, but got\n\n{s}");
            compiler.OnOutput += (sender, output) =>
            {
                executed = true;
                Assert.AreEqual("Hello World!", output, "Intercepted output must match ");
            };

            // Act
            await compiler.Compile(code, true);

            // Assert
            Assert.IsNotNull(compiler.Compilation, "Compilation should be successful");
            Assert.IsTrue(executed, "Execution must emit output.");
        }
    }
}
