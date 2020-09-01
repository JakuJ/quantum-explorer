using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class AstToGateGridTest
    {
        [TestCaseSource(typeof(TestSources), nameof(TestSources.Sources))]
        public async Task ListsDeclaredOperations(string path, string[] operations)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(path);
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            // Act
            await compiler.Compile(code);
            var gates = AstToGateGrid.GetGrids(compiler.Compilation);

            // Assert
            Assert.AreEqual(operations, gates.Keys.ToArray(), "All declared operations should be listed.");
        }

        private class TestSources
        {
            public static IEnumerable Sources
            {
                get
                {
                    (string, string[])[] testCases =
                    {
                        ("Library", new[] { "RandomBit", "RandomInt" }),
                        ("MultipleOperations", new[] { "EntanglePair", "IdentityGate", "Noop", "RandomBit" }),
                    };

                    foreach (var (file, operations) in testCases)
                    {
                        yield return new TestCaseData(file, operations);
                    }
                }
            }
        }
    }
}
