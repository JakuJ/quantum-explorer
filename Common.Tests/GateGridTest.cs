using System;
using System.Linq;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture]
    [Parallelizable]
    public class GateGridTest
    {
        [Test]
        public void AddingGates()
        {
            // Arrange
            var grid = new GateGrid();
            QuantumGate[] gates = SampleGates(4);

            var toAdd = new (QuantumGate G, int X, int Y)[]
            {
                (gates[0], 0, 0),
                (gates[1], 2, 3),
                (gates[2], 5, 5),
                (gates[3], 10, 0),
            };

            var expected = new (QuantumGate G, int X, int Y)[]
            {
                (gates[0], 0, 0),
                (gates[1], 1, 3),
                (gates[2], 2, 5),
                (gates[3], 3, 0),
            };

            // Act
            foreach ((var g, int x, int y) in toAdd)
            {
                grid.AddGate(x, y, g);
            }

            // Assert
            var outGates = grid.Gates.ToArray();
            foreach (var tuple in expected)
            {
                Assert.Contains(tuple, outGates, "Gate at the correct position should be present in the grid.");
            }
        }

        [Test]
        public void CannotAddGatesAtNegativeIndices()
        {
            // Arrange
            GateGrid grid = new();
            const string msg = "Cannot add gates at negative index.";

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => { grid.AddGate(1, -1, new QuantumGate("H")); }, msg);

            Assert.Throws<ArgumentException>(
                () => { grid.AddGate(-2, 3, new QuantumGate("H")); }, msg);
        }

        [Test]
        public void ResizingOnGateAdded()
        {
            // Arrange
            GateGrid grid = new();

            // Act & Assert
            grid.AddGate(4, 4, new QuantumGate("SomeOperation")); // resizes to 5x5 then shrinks to 1x5
            Assert.AreEqual((1, 5), (grid.Width, grid.Height), "Grid should resize correctly");

            grid.AddGate(9, new QuantumGate("SomeOperation")); // resizes to 2x10
            Assert.AreEqual((2, 10), (grid.Width, grid.Height), "Grid should resize correctly");
        }

        [Test]
        public void ShrinkingTo1By1()
        {
            // Arrange
            GateGrid grid = new(5, 5);

            // Act
            grid.AddGate(0, new QuantumGate("TestGate"));

            // Assert
            Assert.AreEqual((1, 1), (grid.Width, grid.Height), "Empty grid should shrink to 1 by 1");
        }

        [Test]
        public void RemovingEmptyColumns()
        {
            // Arrange
            GateGrid grid = new();
            QuantumGate[] gates = SampleGates(5);

            // Act
            grid.AddGate(1, 0, gates[0]); // 0, 0
            grid.AddGate(3, 2, gates[1]); // 1, 2
            grid.AddGate(4, 4, gates[2]); // 2, 4
            grid.AddGate(7, 6, gates[3]); // 3, 6
            grid.AddGate(9, 8, gates[4]); // 4, 8

            var newGates = grid.Gates.ToArray();

            // Assert
            Assert.AreEqual((5, 9), (grid.Width, grid.Height), "Five columns should collapse");

            Assert.Contains((gates[0], 0, 0), newGates, "Gate should change position");
            Assert.Contains((gates[1], 1, 2), newGates, "Gate should change position");
            Assert.Contains((gates[2], 2, 4), newGates, "Gate should change position");
            Assert.Contains((gates[3], 3, 6), newGates, "Gate should change position");
            Assert.Contains((gates[4], 4, 8), newGates, "Gate should change position");
        }

        [Test]
        public void ShrinkingUpToGateExtent()
        {
            // Arrange
            GateGrid grid = new();
            QuantumGate[] gates = SampleGates(4);

            // Act
            grid.AddGate(1, 1, gates[0]);
            grid.AddGate(5, 5, gates[1]);
            grid.AddGate(3, 7, gates[2]);
            grid.AddGate(6, 4, gates[3]);

            // Assert
            Assert.AreEqual((4, 8), (grid.Width, grid.Height), "Empty grid should not be bigger than necessary");
        }

        [Test]
        public void RemovingGates()
        {
            // Arrange
            GateGrid grid = new();
            QuantumGate[] gates = SampleGates(5);

            // Act
            for (var i = 0; i < gates.Length; i++)
            {
                grid.AddGate(i, i, gates[i]);
            }

            // Assert
            Assert.Throws<ArgumentException>(
                () => { grid.RemoveAt(-1, 0); }, "Removing at negative indices is an error.");

            Assert.Throws<ArgumentException>(
                () => { grid.RemoveAt(7, 2); }, "Removing beyond the grid is an error.");

            Assert.Throws<ArgumentException>(
                () => { grid.RemoveAt(1, 3); }, "Removing where there is no gate is an error.");

            Assert.AreEqual(gates[2], grid.RemoveAt(2, 2), "A correct gate should be removed.");
            Assert.AreEqual(gates[4], grid.RemoveAt(3, 4), "A correct gate should be removed.");
            Assert.AreEqual(gates[0], grid.RemoveAt(0, 0), "A correct gate should be removed.");
            Assert.AreEqual(gates[3], grid.RemoveAt(1, 3), "A correct gate should be removed.");
            Assert.AreEqual(gates[1], grid.RemoveAt(0, 1), "A correct gate should be removed.");

            Assert.IsEmpty(grid.Gates, "There should be no gates left");
        }

        [Test]
        public void MovingSingleGate()
        {
            // Arrange
            GateGrid grid = new();
            QuantumGate gate = new("SomeOp", "Testing");

            // Act & Assert
            grid.AddGate(0, 0, gate);

            // different row and column
            grid.MoveGate(0, 0, 1, 1);
            Assert.AreEqual((gate, 0, 1), grid.Gates.First(), "Gate should be moved to a new position");

            // same row
            grid.MoveGate(0, 1, 3, 1);
            Assert.AreEqual((gate, 0, 1), grid.Gates.First(), "Gate should be moved to a new position");

            // same column
            grid.MoveGate(0, 1, 3, 4);
            Assert.AreEqual((gate, 0, 4), grid.Gates.First(), "Gate should be moved to a new position");

            // and back
            grid.MoveGate(0, 4, 0, 0);
            Assert.AreEqual((gate, 0, 0), grid.Gates.First(), "Gate should be moved to a new position");
        }

        [Test]
        public void MovingOutOfBoundsResizesTheGrid()
        {
            // Arrange
            GateGrid grid = new();
            QuantumGate[] gates = SampleGates(2);

            // Act
            grid.AddGate(0, 0, gates[0]);
            grid.AddGate(0, 1, gates[1]);
            grid.MoveGate(0, 1, 1, 0);

            var outGates = grid.Gates.ToArray();

            // Assert
            Assert.AreEqual((gates[0], 0, 0), outGates[0], "First gate should not move");
            Assert.AreEqual((gates[1], 1, 0), outGates[1], "Gate should be moved to a new position");
            Assert.AreEqual((2, 1), (grid.Width, grid.Height), "Grid should resize");
        }

        [TestCase(0, "01234")]
        [TestCase(1, "01234")]
        [TestCase(2, "10234")]
        [TestCase(3, "12034")]
        [TestCase(4, "12304")]
        [TestCase(5, "12340")]
        public void CanInsertBetweenTwoGatesOnTheSameRow(int targetColumn, string expected)
        {
            // Arrange
            GateGrid grid = new(1, 1);
            QuantumGate[] gates = SampleGates(5);

            // Act
            for (var i = 0; i < gates.Length; i++)
            {
                grid.AddGate(i, 0, gates[i]); // 01234
            }

            grid.MoveGate(0, 0, targetColumn, 0);
            var newGates = grid.Gates.ToArray();

            // Assert
            for (var i = 0; i < gates.Length; i++)
            {
                // assuming left-to-right, top-to-bottom order of grid.Gates
                Assert.AreEqual((gates[expected[i] - '0'], i, 0), newGates[i], "Gates should be in correct order");
            }
        }

        [Test]
        public void MovingOntoAnotherGateInASeparateColumnMovesItToTheRight()
        {
            // Arrange
            GateGrid grid = new(1, 2);
            QuantumGate[] gates = SampleGates(2);

            // Act
            grid.AddGate(0, 0, gates[0]);
            grid.AddGate(0, 1, gates[1]);

            grid.MoveGate(0, 0, 0, 1);
            var newGates = grid.Gates.ToArray();

            // Assert
            Assert.AreEqual(new[] { (gates[0], 0, 1), (gates[1], 1, 1) }, newGates, "Both gates should be on the second row.");
        }

        [Test]
        public void ComplexMovingScenarios()
        {
            // Arrange
            GateGrid grid = new(3, 3);
            QuantumGate[] gates = SampleGates(5);

            var expected1 = new[] { (gates[0], 0, 0), (gates[2], 1, 0), (gates[1], 2, 0), (gates[3], 2, 1), (gates[4], 2, 2) };
            var expected2 = new[] { (gates[1], 0, 0), (gates[0], 1, 0), (gates[2], 2, 0), (gates[3], 3, 1), (gates[4], 3, 2) };

            // Act
            grid.AddGate(0, 0, gates[0]);
            grid.AddGate(1, 0, gates[1]);
            grid.AddGate(2, 0, gates[2]);
            grid.AddGate(1, 1, gates[3]);
            grid.AddGate(1, 2, gates[4]);

            /* 0 1 2
               _ 3 _
               _ 4 _ */
            grid.MoveGate(2, 0, 1, 0);
            /* 0 2 1
               _ _ 3
               _ _ 4 */

            // Assert
            Assert.AreEqual(expected1, grid.Gates.ToArray(), "Gates should be in a new order");
            Assert.AreEqual((3, 3), (grid.Width, grid.Height), "Grid size should remain unchanged");

            // Act

            /* 0 2 1
               _ _ 3
               _ _ 4 */
            grid.MoveGate(2, 0, 0, 0);
            /* 1 0 2 _
               _ _ _ 3
               _ _ _ 4 */

            // Assert
            Assert.AreEqual(expected2, grid.Gates.ToArray(), "Gates should be in a new order");
            Assert.AreEqual((4, 3), (grid.Width, grid.Height), "Grid should expand to accomodate a new column");

            // Act

            /* 1 0 2 _
               _ _ _ 3
               _ _ _ 4 */
            grid.MoveGate(0, 0, 3, 0);
            /* 0 2 1
               _ _ 3
               _ _ 4 */

            // Assert
            Assert.AreEqual(expected1, grid.Gates.ToArray(), "Gates should be back in the previous order");
            Assert.AreEqual((3, 3), (grid.Width, grid.Height), "Grid size should go back to 3 by 3");
        }

        [Test]
        public void RemovingMultiQubitGates()
        {
            // Arrange
            GateGrid grid = new();

            QuantumGate[] gates =
            {
                new("Test", "TestNs"),
                new("Test", "TestNs", 1),
                new("Test", "TestNs", 2),
            };

            foreach ((int i, QuantumGate gate) in gates.Enumerate())
            {
                grid.AddGate(0, i, gate);
            }

            // Act & Assert
            grid.RemoveAt(0, 1);
            Assert.AreEqual(2, grid.Gates.Count(), "There should be 2 gates left");

            grid.RemoveAt(0, 0);
            grid.RemoveAt(0, 2);
            Assert.IsEmpty(grid.Gates, "There should be no more gates in the grid");
        }

        [Test]
        public void SortingNames()
        {
            // Arrange
            GateGrid grid = new();
            foreach ((int ix, string name) in new[] { "C", "A", "B", "E", "D" }.Enumerate())
            {
                grid.SetName(ix, name);
                grid.AddGate(ix, new QuantumGate(name));
            }

            // Act
            grid.SortRowsByQubitIds();

            // Assert
            foreach ((int ix, (string name, int column)) in new[] { ("A", 1), ("B", 2), ("C", 0), ("D", 4), ("E", 3) }.Enumerate())
            {
                Assert.AreEqual(name, grid.Names[ix], "Qubit IDs should be in correct positions");
                Assert.AreEqual(name, grid.At(column, ix)!.Value.Name, "Gates should be in correct positions");
            }
        }

        [Test]
        public void GateGridsAreNotEqual()
        {
            // Arrange
            var gateGrid1 = new GateGrid(1, 2);
            string other = "Not a GateGrid";

            // Act && Assert
            Assert.False(gateGrid1.Equals(other));
        }

        private QuantumGate[] SampleGates(int howMany) => Enumerable.Range(0, howMany).Select(i => new QuantumGate($"Op{i}")).ToArray();
    }
}
