using System;
using System.Linq;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class GateGridTest
    {
        private static class Sources
        {
            public static GateGrid[] Cases => new[]
            {
                new GateGrid(1),
                new GateGrid(5),
                new GateGrid(new[]
                {
                    new[]
                    {
                        new QuantumGate("X"),
                        new QuantumGate("H"),
                        new QuantumGate("Z"),
                    },
                    new[]
                    {
                        new QuantumGate("MResetZ"),
                        new QuantumGate("Y"),
                    },
                }),
            };
        }

        [TestCaseSource(typeof(Sources), nameof(Sources.Cases))]
        public void StructuralEqualityWithClone(GateGrid grid)
        {
            // Arrange
            object clone = grid.Clone();

            // Act & Assert
            Assert.AreSame(clone, clone, "A gate grid is reference-equal with itself");
            Assert.AreEqual(grid, clone, "A gate grid and its clone should be structurally equal");
            Assert.True(grid == (GateGrid)clone, "Comparison operator should work");
            Assert.True(grid.GetHashCode() != clone.GetHashCode(), "Structurally equal object should still have different hashes.");
        }

        [Test]
        public void InequalityTests()
        {
            // Arrange
            var grid1 = new GateGrid(3);
            var grid2 = new GateGrid(4);
            var grid3 = new GateGrid(4);

            // Act & Assert
            Assert.AreNotEqual(grid1, grid2, "Grids with different number of qubits should not compare equal.");
            Assert.True(grid1 != grid2, "Inequality operator should work.");
            Assert.AreNotEqual(grid1, null, "A grid can never be equal to null.");

            grid2.AddGate(1, new QuantumGate("H"));
            Assert.AreNotEqual(grid2, grid3, "Grids with different number of gates in a lane should not compare equal.");

            grid3.AddGate(1, new QuantumGate("X"));
            Assert.AreNotEqual(grid2, grid3, "Grids with different gates should not compare equal.");
        }

        [Test]
        public void EmptyGridHasNoLayers()
        {
            // Arrange
            var grid = new GateGrid(4);

            // Act
            var layers = grid.Layers;

            // Assert
            Assert.IsEmpty(layers, "Empty grid should have no layers");
        }

        [Test]
        public void QubitDoesNotExist()
        {
            // Arrange
            var grid = new GateGrid(2);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                grid.AddGate(2, new QuantumGate("H")); // off by one
            });
        }

        [TestCase(1, 3)]
        [TestCase(4, 2)]
        [TestCase(10, 5)]
        public void AddLayersOfSingleQubitGatesToMultipleQubitGrids(int qubits, int layers)
        {
            // Arrange
            int gatesToAdd = layers;
            var grid = new GateGrid(qubits);

            // Act
            for (var qubit = 0; qubit < qubits; qubit++)
            {
                for (var i = 0; i < gatesToAdd; i++)
                {
                    grid.AddGate(qubit, new QuantumGate("H"));
                }

                // Triangle-like pattern, first gate gets N gates, then next N - 1 and so on
                gatesToAdd -= 1;
            }

            // Assert
            Assert.AreEqual(layers, grid.Layers.Count(), "There should be as many layers as the longest lane has.");
        }
    }
}
