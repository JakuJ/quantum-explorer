using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AstTransformations;
using Common;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class AstTransformationsTest
    {
        private static class TestSources
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
                    yield return new object[]
                    {
                        "AllocateOne",
                        new (string, int, int)[] { },
                        new[] { "qubit" },
                    };

                    yield return new object[]
                    {
                        "AllocateFive",
                        new (string, int, int)[] { },
                        new[] { "qs[0]", "qs[1]", "qs[2]", "qs[3]", "qs[4]" },
                    };

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
                        new[] { "q" },
                    };
                    yield return new object[]
                    {
                        "AllocateFiveAndApplyGates",
                        new[]
                        {
                            ("H", 0, 0),
                            ("X", 1, 1),
                            ("Y", 2, 1),
                            ("Z", 3, 4),
                        },
                        new[] { "qs[0]", "qs[1]", "qs[2]", "qs[3]", "qs[4]" },
                    };
                }
            }

            internal static Dictionary<string, GateGrid> AllocatedQubitOpsGrids { get; } = ProcessAllocatedQubitOps().WaitAndUnwrapException();

            private static async Task<Dictionary<string, GateGrid>> ProcessAllocatedQubitOps()
            {
                string code = await Helpers.GetSourceFile("AllocatedQubitOps");
                var compiler = new QsCompiler(Helpers.ConsoleLogger);

                await compiler.Compile(code);
                return FromQSharp.GetGates(compiler.Compilation);
            }
        }

        [TestCaseSource(typeof(TestSources), nameof(TestSources.DeclaredOps))]
        public async Task ListsDeclaredOperations(string path, string[] operations)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(path);
            var compiler = new QsCompiler(Helpers.ConsoleLogger);
            var qualifiedNames = operations.Select(x => $"{path}.{x}").ToArray();

            // Act
            await compiler.Compile(code);
            var gates = FromQSharp.GetGates(compiler.Compilation);

            // Assert
            Assert.AreEqual(qualifiedNames, gates.Keys.ToArray(), "All declared operations should be listed.");
        }

        [Test]
        public async Task ProcessesMultipleNamespaces()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("MultipleNamespaces");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);
            string[] qualifiedNames = new[] { "Ns1.Op1", "Ns2.Op1", "Ns2.Op2", "Ns3.Op2" };

            // Act
            await compiler.Compile(code);
            var gates = FromQSharp.GetGates(compiler.Compilation);

            // Assert
            Assert.AreEqual(qualifiedNames, gates.Keys.ToArray(), "All declared operations should be listed.");
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
        public void ExtractsQuantumGates(string operation, (string, int, int)[] gates, string[] names)
        {
            // Arrange (& Act)
            var grids = TestSources.AllocatedQubitOpsGrids;
            operation = $"AllocatedQubitOps.{operation}";

            // Assert
            foreach ((QuantumGate gate, int x, int y) in grids[operation].Gates)
            {
                Assert.Contains((gate.Name, x, y), gates, "Gate should be present at a given position");
            }

            Assert.AreEqual(names, grids[operation].Names, "Assigned qubit identifiers should be correct");
        }

        [Test]
        public async Task LocallyDeclaredOperations()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("LocalOps");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            // Act
            await compiler.Compile(code);
            GateGrid? grid = FromQSharp.GetGates(compiler.Compilation)["LocalOps.MotherOp"];

            // Assert
            Assert.AreEqual(1, grid.Gates.Count(), "There should be 1 gate in the grid");
            
            (var gate, int x, int y) = grid.Gates.First();
            Assert.AreEqual(("DoesSomethingWithASingleQubit", 0, 0), (gate.Name, x, y), "A local gate should be detected");
        }
    }
}
