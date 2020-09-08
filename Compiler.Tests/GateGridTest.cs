using System.Linq;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class GateGridTest
    {
        [Theory]
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
