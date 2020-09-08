using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class AstToGateGridTest
    {
        private class TestSources
        {
            internal static object[] DeclaredOps => new object[]
            {
                new object[] { "Library", new[] { "RandomBit", "RandomInt" } },
                new object[] { "MultipleOperations", new[] { "EntanglePair", "IdentityGate", "Noop", "RandomBit" } },
            };

            internal static object[] GateGrids => new object[]
            {
                new object[] { "AllocateOne", new GateGrid(1) },
                new object[] { "AllocateOneAndApplyH", new GateGrid(new[] { new[] { new QuantumGate("H") } }) },
                new object[] { "AllocateTwo", new GateGrid(2) },
                new object[]
                {
                    "AllocateOneAndApplyGates", new GateGrid(new[]
                    {
                        new[]
                        {
                            new QuantumGate("H"),
                            new QuantumGate("MResetZ"),
                        },
                    }),
                },

                // TODO: Quantum teleportation circuit with 'Controlled' functor support
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
            var gates = AstToGateGrid.GetGrids(compiler.Compilation);

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
            var gates = AstToGateGrid.GetGrids(compiler.Compilation);

            // Assert
            Assert.AreEqual(grid, gates[operation], "Quantum circuit should be extracted correctly.");
        }
    }
}
