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
                new object[] { "MultipleOperations", new[] { "EntanglePair", "IdentityGate", "Noop", "RandomBit" } },
                new object[]
                {
                    "AllocatedQubitOps",
                    new[] { "AllocateFive", "AllocateFiveAndApplyGates", "AllocateOne", "AllocateOneAndApplyGates" },
                },
            };

            internal static object[] GateGrids => new object[]
            {
                new object[] { "AllocateOne", new GateGrid(1) },
                new object[] { "AllocateFive", new GateGrid(5) },
                new object[]
                {
                    "AllocateOneAndApplyGates", new GateGrid(new[]
                    {
                        new[]
                        {
                            new QuantumGate("H"),
                            new QuantumGate("Z"),
                            new QuantumGate("X"),
                            new QuantumGate("MResetZ"),
                        },
                    }),
                },
                new object[]
                {
                    "AllocateFiveAndApplyGates", new GateGrid(new[]
                    {
                        new[] { new QuantumGate("H") },
                        new[] { new QuantumGate("X"), new QuantumGate("Y") },
                        new QuantumGate[] { },
                        new QuantumGate[] { },
                        new[] { new QuantumGate("Z") },
                    }),
                },
            };
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

        [TestCaseSource(typeof(TestSources), nameof(TestSources.GateGrids))]
        public async Task ExtractsQuantumGates(string operation, GateGrid grid)
        {
            // Arrange
            string code = await Helpers.GetSourceFile("AllocatedQubitOps");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            // Act
            await compiler.Compile(code);
            var gates = FromQSharp.GetGates(compiler.Compilation);

            // Assert
            Assert.AreEqual(grid, gates[operation], "Quantum circuit should be extracted correctly.");
        }
    }
}
