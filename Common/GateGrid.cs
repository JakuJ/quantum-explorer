using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>Represents a grid of quantum gates.</summary>
    public class GateGrid
    {
        private readonly List<List<QuantumGate?>> grid = new();

        /// <summary>Initializes a new instance of the <see cref="GateGrid"/> class.</summary>
        public GateGrid() { }

        /// <inheritdoc cref="GateGrid()"/>
        /// <param name="width">The length of the longest lane in this circuit.</param>
        /// <param name="height">The number of qubits in this circuit.</param>
        public GateGrid(int height, int width) => Expand(width, height);

        /// <summary>Gets the array of identifiers associated with the qubits.</summary>
        public List<string?> Names { get; private set; } = new();

        /// <summary>Gets the length of the longest lane in this grid.</summary>
        public int Width => grid.Count;

        /// <summary>Gets the number of qubits in this grid.</summary>
        public int Height
        {
            get
            {
                int? ix = grid.FirstOrDefault()?.Count;
                return ix ?? Names.TakeWhile(x => x != null).Count();
            }
        }

        /// <summary>Gets all gates in this grid.</summary>
        public IEnumerable<(QuantumGate Gate, int X, int Y)> Gates
        {
            get
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        QuantumGate? gate = grid[x][y];
                        if (gate.HasValue)
                        {
                            yield return (gate.Value, x, y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the gate at a given position.
        /// </summary>
        /// <param name="x">Row.</param>
        /// <param name="y">Column.</param>
        /// <returns>A gate if present at the position, null otherwise.</returns>
        public QuantumGate? At(int x, int y) => grid.ElementAtOrDefault(x)?.ElementAtOrDefault(y);

        /// <summary>Set an identifier of a qubit.</summary>
        /// <param name="qubit">The index of the qubit.</param>
        /// <param name="name">The identifier to assign to the qubit.</param>
        public void SetName(int qubit, string name)
        {
            if (!BoundsCheck(0, qubit))
            {
                Expand(0, qubit + 1 - Height);
            }

            Names[qubit] = name;
        }

        /// <summary>Add a gate to the grid.</summary>
        /// <param name="y">Row (quit) index.</param>
        /// <param name="gate">The gate to place on the grid.</param>
        public void AddGate(int y, QuantumGate gate) => AddGate(Width, y, gate);

        /// <summary>Add a gate to the grid.</summary>
        /// <param name="x">Column index.</param>
        /// <param name="y">Row (quit) index.</param>
        /// <param name="gate">The gate to place on the grid.</param>
        public void AddGate(int x, int y, QuantumGate gate)
        {
            if (!BoundsCheck(x, y))
            {
                Expand(Math.Max(Width, x + 1) - Width, Math.Max(Height, y + 1) - Height);
            }

            if (grid[x][y] != null)
            {
                InsertColumn(x);
            }

            grid[x][y] = gate;

            Shrink();
        }

        /// <summary>Insert an empty row above the one with the provided index.</summary>
        /// <param name="rowBelow">Index of the row directly below the inserted one.</param>
        /// <param name="qubitId">Identifier for the inserted row.</param>
        public void InsertRow(int rowBelow, string qubitId)
        {
            Names.Insert(rowBelow, qubitId);
            grid.ForEach(col => col.Insert(rowBelow, null));
        }

        /// <summary>Insert an empty column before the one with the provided index.</summary>
        /// <param name="columnRight">Index of the column right to the inserted one.</param>
        public void InsertColumn(int columnRight)
        {
            grid.Insert(columnRight, EmptyColumn());
        }

        /// <summary>Removes the gate at a given position.</summary>
        /// <param name="x">The index of the column.</param>
        /// <param name="y">The index of the qubit.</param>
        /// <param name="moving">Whether this removal is part of a moving action.</param>
        /// <returns>The removed gate, if any.</returns>
        public QuantumGate RemoveAt(int x, int y, bool moving = false)
        {
            if (!BoundsCheck(x, y))
            {
                throw new ArgumentException("Trying to remove a gate outside the grid");
            }

            QuantumGate? gate = grid[x][y];

            if (!gate.HasValue)
            {
                throw new ArgumentException($"There is no gate at location ({x}, {y})");
            }

            grid[x][y] = null;

            if (moving)
            {
                return gate.Value;
            }

            Shrink();
            return gate.Value;
        }

        /// <summary>
        /// Moves a gate at a given position to the another position.
        /// If the space is already occupied, move the whole column at that position to the right.
        /// </summary>
        /// <param name="xFrom">The index of the original gate's column.</param>
        /// <param name="yFrom">The index of the original gate's qubit.</param>
        /// <param name="xTo">The index of the target column.</param>
        /// <param name="yTo">The index of target qubit.</param>
        public void MoveGate(int xFrom, int yFrom, int xTo, int yTo)
        {
            QuantumGate gate = RemoveAt(xFrom, yFrom, true);
            AddGate(xTo, yTo, gate);
        }

        /// <summary>Remove empty rows (qubits) from the grid.</summary>
        public void RemoveEmptyRows()
        {
            int[] indices = Names
                           .Enumerate()
                           .Where(x => x.Item == null)
                           .Select(x => x.Index)
                           .Reverse()
                           .ToArray();

            Names.RemoveAll(x => x == null);

            foreach (var column in grid)
            {
                foreach (int index in indices)
                {
                    column.RemoveAt(index);
                }
            }
        }

        /// <summary>Sort rows in the grid by their corresponding qubit IDs, alphabetically.</summary>
        public void SortRowsByQubitIds()
        {
            (int Index, string? Item)[] sorted = Names.Enumerate().OrderBy(x => x.Item).ToArray();
            Names = sorted.Select(x => x.Item).ToList();

            int[] indices = sorted.Select(x => x.Index).ToArray();

            foreach (var column in grid)
            {
                QuantumGate?[] copy = column.ToArray();
                foreach ((int index, int newIndex) in indices.Enumerate())
                {
                    column[index] = copy[newIndex];
                }
            }
        }

        private bool BoundsCheck(int x, int y)
        {
            if (x < 0 || y < 0)
            {
                throw new ArgumentException($"Position ({x}, {y}) is out of bounds of the grid");
            }

            return x < Width && y < Height;
        }

        private void Shrink()
        {
            // Collapse empty columns
            grid.RemoveAll(col => col.TrueForAll(x => x == null));

            // Remove unused qubits
            var max = 0;

            for (int y = Height - 1; y >= 0; y--)
            {
                if (Names[y] != null)
                {
                    max = y + 1; // height, not index
                    break;
                }

                for (var x = 0; x < Width; x++)
                {
                    if (grid[x][y] != null)
                    {
                        max = y + 1; // height, not index
                        goto endLoop;
                    }
                }
            }

            endLoop:

            if (max != Height)
            {
                Names.Truncate(max);
                grid.ForEach(col => col.Truncate(max));
            }
        }

        private void Expand(int plusWidth, int plusHeight)
        {
            for (var i = 0; i < plusWidth; i++)
            {
                grid.Add(EmptyColumn());
            }

            if (plusHeight > 0)
            {
                Names.AddRange(Enumerable.Repeat<string?>(null, plusHeight));
                grid.ForEach(col => col.AddRange(EmptyColumn(plusHeight)));
            }
        }

        private List<QuantumGate?> EmptyColumn(int? height = null) => Enumerable.Repeat<QuantumGate?>(null, height ?? Height).ToList();
    }
}
