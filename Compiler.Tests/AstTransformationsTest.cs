using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AstTransformations;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class AstTransformationsTest
    {
        private class TestSources
        {
            internal static object[] DeclaredOps => new object[]
            {
                new object[] { "Library", new[] { "RandomBit", "RandomInt" } },
                new object[] { "MultipleOperations", new[] { "EntanglePair", "IdentityGate", "NoOp", "RandomBit" } },
                new object[]
                {
                    "AllocatedQubitOps",
                    new[] { "AllocateFive", "AllocateFiveAndApplyGates", "AllocateOne", "AllocateOneAndApplyGates" },
                },
            };

            internal static IEnumerable<object> GateGrids
            {
                get
                {
                    yield return new object[] { "AllocateOne", new (string, int, int)[] { } };
                    yield return new object[] { "AllocateFive", new (string, int, int)[] { } };
                    yield return new object[]
                    {
                        "AllocateOneAndApplyGates",
                        new[]
                        {
                            ("H", 0, 0),
                            ("Z", 1, 0),
                            ("X", 2, 0),
                            ("MResetZ", 3, 0),
                        },
                    };
                    yield return new object[]
                    {
                        "AllocateFiveAndApplyGates",
                        new[]
                        {
                            ("H", 0, 0),
                            ("X", 0, 1),
                            ("Y", 1, 1),
                            ("Z", 0, 4),
                        },
                    };
                }
            }
        }

        [TestCaseSource(typeof(TestSources), nameof(TestSources.DeclaredOps))]
        public async Task ListsDeclaredOperations(string path, string[] operations)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(path);
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            // Act
            await compiler.Compile(code);
            var gates = FromQSharp.GetGates(compiler.Compilation);

            // Assert
            Assert.AreEqual(operations, gates.Keys.ToArray(), "All declared operations should be listed.");
        }

        [TestCase("EmptyFile")]
        [TestCase("EmptyNamespace")]
        [TestCase("SingleEmptyFunction")]
        public async Task FindsNoOperations(string path)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(path);
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            // Act
            await compiler.Compile(code);
            var gates = FromQSharp.GetGates(compiler.Compilation);

            // Assert
            Assert.IsEmpty(gates, "Traversing the AST should find no operations.");
        }

        [TestCaseSource(typeof(TestSources), nameof(TestSources.GateGrids))]
        public async Task ExtractsQuantumGates(string operation, (string, int, int)[] gates)
        {
            // Arrange
            string code = await Helpers.GetSourceFile("AllocatedQubitOps");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            // Act
            await compiler.Compile(code);
            var grid = FromQSharp.GetGates(compiler.Compilation);

            // Assert
            foreach ((QuantumGate gate, int x, int y) in grid[operation].Gates)
            {
                Assert.Contains((gate.Name, x, y), gates, "Gate should be present at a given position");
            }
        }
    }
}
