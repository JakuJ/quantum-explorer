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

            // Act
            await compiler.Compile(code);

            // Assert
            Assert.IsNotNull(compiler.Compilation, "Compilation should be successful");
        }
    }
}
