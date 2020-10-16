using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Compiler
{
    /// <inheritdoc cref="System.IEquatable{GateGrid}" />
    /// <summary>A simple class representing a grid of quantum gates.</summary>
    public class GateGrid
    {
        private List<QuantumGate?[]> grid = new List<QuantumGate?[]>();

        /// <summary>Initializes a new instance of the <see cref="GateGrid"/> class.</summary>
        public GateGrid() { }

        /// <inheritdoc cref="GateGrid()"/>
        /// <param name="width">The length of the longest lane in this circuit.</param>
        /// <param name="height">The number of qubits in this circuit.</param>
        public GateGrid(int height, int width) => Expand(width, height);

        /// <summary>Gets the array of identifiers associated with the qubits.</summary>
        public string?[] Names { get; private set; } = new string[] { };

        /// <summary>Gets the length of the longest lane in this grid.</summary>
        public int Width => grid.Count;

        /// <summary>Gets the number of qubits in this grid.</summary>
        public int Height
        {
            get
            {
                int? ix = grid.FirstOrDefault()?.Length;
                return ix ?? Names.TakeWhile(x => x != null).Count();
            }
        }

        /// <summary>Return which row on the grid corresponds to a given qubit identifier.</summary>
        /// <param name="name">The identifier to look for.</param>
        /// <returns>Index of the qubit corresponding to this name.</returns>
        public int IndexOfName(string name)
        {
            int ix = Array.IndexOf(Names, name);

            if (ix == -1)
            {
                throw new ArgumentException(nameof(name));
            }

            return ix;
        }

        /// <summary>Gets all gates in this grid.</summary>
        public IEnumerable<(QuantumGate Gate, int X, int Y)> Gates
        {
            get
            {
                QuantumGate? gate;
                var seen = new bool[Width, Height];

                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        if (!seen[x, y] && (gate = At(x, y)) != null)
                        {
                            yield return (gate, x, y);
                            for (var i = 0; i < gate.Height; i++)
                            {
                                seen[x, y + i] = true;
                            }
                        }
                    }
                }
            }
        }

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
            if (!BoundsCheck(x, y + gate.Height - 1))
            {
                Expand(Math.Max(Width, x + 1) - Width, Math.Max(Height, y + gate.Height) - Height);
            }

            if (At(x, y) != null)
            {
                grid.Insert(x, new QuantumGate?[Height]);
            }

            for (var i = 0; i < gate.Height; i++)
            {
                At(x, y + i) = gate;
            }

            Shrink();
        }

        /// <summary>Removes the gate at a given position.</summary>
        /// <param name="x">The index of the column.</param>
        /// <param name="y">The index of the qubit.</param>
        /// <param name="shrink">Whether to normalize the grid afterwards.</param>
        /// <returns>The removed gate, if any.</returns>
        public QuantumGate RemoveAt(int x, int y, bool shrink = true)
        {
            if (!BoundsCheck(x, y))
            {
                throw new ArgumentOutOfRangeException("Trying to remove a gate outside the grid");
            }

            var gate = At(x, y);

            if (gate == null)
            {
                throw new ArgumentException($"There is no gate at location ({x}, {y})");
            }

            if (gate.Height > 1)
            {
                if (!BoundsCheck(x, y + gate.Height - 1) || At(x, y + gate.Height - 1) != gate)
                {
                    throw new ArgumentException($"Position ({x}, {y}) is not a start of a gate, but rather in a middle of a multi-qubit one");
                }
            }

            for (int i = 0; i < gate.Height; i++)
            {
                At(x, y + i) = null;
            }

            if (shrink)
            {
                Shrink();
            }

            return gate;
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
            var gate = RemoveAt(xFrom, yFrom, shrink: false);
            AddGate(xTo, yTo, gate);
        }

        /// <summary>Shrinks the grid to occupy the least number of squares in each direction.</summary>
        public void Shrink()
        {
            // Collapse empty columns
            grid = grid.Where(col => col.Any(x => x != null)).ToList();

            // Remove unused qubits
            var max = 0;

            for (int y = Height - 1; y >= 0; y--)
            {
                if (Names[y] != null)
                {
                    max = y + 1; // height, not index
                    break;
                }

                for (int x = 0; x < Width; x++)
                {
                    if (At(x, y) != null)
                    {
                        max = y + 1; // height, not index
                        goto endLoop;
                    }
                }
            }

        endLoop:

            if (max != Height)
            {
                grid = grid.Select(col => col.Take(max).ToArray()).ToList();
                Names = Names.Take(max).ToArray();
            }
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage] // Only used as a placeholder until the Compositor is done
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            for (var y = 0; y < Height; y++)
            {
                builder.Append($"{Names[y]}:");
                for (var x = 0; x < Width; x++)
                {
                    builder.Append(" " + (At(x, y)?.Name ?? "_"));
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private ref QuantumGate? At(int x, int y) => ref grid.ElementAt(x)[y];

        private bool BoundsCheck(int x, int y)
        {
            if (x < 0 || y < 0)
            {
                throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds of the grid");
            }

            return x < Width && y < Height;
        }

        private void Expand(int plusWidth, int plusHeight)
        {
            if (plusHeight == 0)
            {
                for (int i = 0; i < plusWidth; i++)
                {
                    grid.Add(new QuantumGate?[Height]);
                }
            }
            else
            {
                Names = Names
                    .Concat(Enumerable
                            .Repeat<string?>(null, plusHeight))
                    .ToArray();

                grid = grid
                        .Select(col => col
                                        .Concat(Enumerable.Repeat<QuantumGate?>(null, plusHeight))
                                        .ToArray())
                        .Concat(Enumerable
                                .Range(0, plusWidth)
                                .Select(_ => new QuantumGate?[Height + plusHeight]))
                        .ToList();
            }
        }
    }
}
