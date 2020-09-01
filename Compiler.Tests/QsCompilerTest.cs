using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class QsCompilerTest
    {
        [Test]
        public async Task CompilesExampleCodeWithoutWarnings()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("Library");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            // Act
            await compiler.Compile(code);

            // Assert
            Assert.AreEqual(0, compiler.Diagnostics.Count(), "There should be no warnings or errors");
        }
    }
}
