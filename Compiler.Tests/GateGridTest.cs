using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class GateGridTest
    {
        [DatapointSource]
        private IEnumerable<(GateGrid Grid1, GateGrid Grid2)> Grids
        {
            get
            {
                const int howMany = 20;
                var rng = new Random();

                for (var i = 0; i < howMany; i++)
                {
                    int qubits = rng.Next(10);

                    var grid1 = new GateGrid(qubits);
                    var grid2 = new GateGrid(qubits);

                    for (var q = 0; q < qubits; q++)
                    {
                        int gates = rng.Next(10);
                        for (var g = 0; g < gates; g++)
                        {
                            var gate = new QuantumGate("X");
                            grid1.AddGate(q, gate);
                            grid2.AddGate(q, gate);
                        }
                    }

                    yield return (grid1, grid2);
                }
            }
        }

        [Theory]
        public void StructuralComparison((GateGrid Gate1, GateGrid Gate2) grid)
        {
            Assert.AreEqual(grid.Gate1, grid.Gate2, "Gate grids should be structurally equal");
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
