using System;
using System.Linq;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class GateGridTest
    {
        [Test]
        public void AddingGates()
        {
            // Arrange
            var grid = new GateGrid(1, 1);
            var toAdd = new (QuantumGate G, int X, int Y)[]
            {
                (new QuantumGate("H", "Microsoft.Intrinsic"), 0, 0),
                (new QuantumGate("X", "Microsoft.Intrinsic"), 2, 3),
                (new QuantumGate("SomeOp", "Testing"), 5, 5),
                (new QuantumGate("SomeOtherOp", "Ns2"), 10, 0),
            };

            // Act
            foreach ((var g, int x, int y) in toAdd)
            {
                grid.AddGate(x, y, g);
            }

            // Assert
            var outGates = grid.Gates.ToArray();
            foreach (var tuple in toAdd)
            {
                Assert.Contains(tuple, outGates, "Gate at the correct position should be present in the grid.");
            }
        }

        [Test]
        public void CannotAddGatesAtNegativeIndices()
        {
            // Arrange
            var grid = new GateGrid(1, 1);
            string msg = "Cannot add gates at negative index.";

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                grid.AddGate(1, -1, new QuantumGate("H"));
            }, msg);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                grid.AddGate(-2, 3, new QuantumGate("H"));
            }, msg);
        }

        [Test]
        public void AddingOverlappingGatesFails()
        {
            // Arrange
            var grid = new GateGrid(1, 1);
            string msg = "Trying to place a gate that overlaps with another one should throw.";

            // Act
            grid.AddGate(0, 0, new QuantumGate("H"));
            grid.AddGate(1, 0, new QuantumGate("SomeMultiQubitOperation", "SomeNamespace", 5));

            // Assert
            Assert.Throws<ArgumentException>(() => { grid.AddGate(0, 0, new QuantumGate("X")); }, msg);
            Assert.Throws<ArgumentException>(() => { grid.AddGate(1, 2, new QuantumGate("X")); }, msg);
            Assert.Throws<ArgumentException>(() => { grid.AddGate(1, 3, new QuantumGate("X", "Ns", 3)); }, msg);
        }

        [Test]
        public void ResizingOnGateAdded()
        {
            // Arrange
            var grid = new GateGrid(1, 1);

            // Act & Assert
            grid.AddGate(4, 4, new QuantumGate("SomeOperation")); // resizes to 5x5
            Assert.AreEqual((5, 5), (grid.Width, grid.Height), "Grid should resize correctly");

            grid.AddGate(0, 5, new QuantumGate("SomeOperation", "Test", 5)); // resizes to 5x10
            Assert.AreEqual((5, 10), (grid.Width, grid.Height), "Grid should resize correctly");
        }

        [Test]
        public void ShrinkingTo1By1()
        {
            // Arrange
            var grid = new GateGrid(5, 5);

            // Act
            grid.Shrink();

            // Assert
            Assert.AreEqual((1, 1), (grid.Width, grid.Height), "Empty grid should shrink to 1x1");
        }

        [Test]
        public void ShrinkingUpToGateExtent()
        {
            // Arrange
            var grid = new GateGrid(10, 10);

            // Act
            grid.AddGate(1, 1, new QuantumGate("SomeOp"));
            grid.AddGate(5, 5, new QuantumGate("SomeOp"));
            grid.AddGate(3, 7, new QuantumGate("SomeOp"));
            grid.AddGate(6, 4, new QuantumGate("SomeOp"));
            grid.Shrink();

            // Assert
            Assert.AreEqual((7, 8), (grid.Width, grid.Height), "Empty grid should shrink to accomodate all gates");
        }

        [Test]
        public void SettingQubitIdentifiers()
        {
            // Arrange
            var grid = new GateGrid(3, 1);

            // Act
            grid.SetName(0, "qs[0]");
            grid.SetName(1, "qs[1]");
            grid.SetName(2, "SomeQ");

            // maybe try to also expand the grid to see if names persist
            grid.AddGate(2, new QuantumGate("H"));
            grid.AddGate(6, new QuantumGate("H"));

            // Assert
            Assert.AreEqual("qs[0]", grid.Names[0], "An identifier once set should be persistent");
            Assert.AreEqual("qs[1]", grid.Names[1], "An identifier once set should be persistent");
            Assert.AreEqual("SomeQ", grid.Names[2], "An identifier once set should be persistent");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                grid.SetName(10, "SomeOtherQ");
            }, "Cannot set names to nonexistent qubits");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                grid.SetName(-1, "SomeOtherQ");
            }, "Cannot set names to negative index qubits");
        }
    }
}
