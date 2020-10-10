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
        private QuantumGate?[,] grid;

        /// <summary>Initializes a new instance of the <see cref="GateGrid"/> class.</summary>
        /// <param name="width">The length of the longest lane in this circuit.</param>
        /// <param name="height">The number of qubits in this circuit.</param>
        public GateGrid(int height = 10, int width = 10)
        {
            grid = new QuantumGate?[width, height];
            Names = Enumerable.Range(0, height).Select(x => $"Q{x}").ToArray();
        }

        /// <summary>Gets the array of identifiers associated with the qubits.</summary>
        public string[] Names { get; private set; }

        /// <summary>Gets the length of the longest lane in this grid.</summary>
        public int Width => grid.GetLength(0);

        /// <summary>Gets the number of qubits in this grid.</summary>
        public int Height => grid.GetLength(1);

        /// <summary>Gets all gates in this grid.</summary>
        public IEnumerable<(QuantumGate Gate, int X, int Y)> Gates
        {
            get
            {
                var seen = new bool[Width, Height];
                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        QuantumGate? gate = grid[x, y];
                        if (!seen[x, y] && gate != null)
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
            if (qubit < 0 || qubit >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(qubit));
            }

            Names[qubit] = name;
        }

        /// <summary>Add a gate to the grid.</summary>
        /// <param name="y">Row (quit) index.</param>
        /// <param name="gate">The gate to place on the grid.</param>
        public void AddGate(int y, QuantumGate gate)
        {
            if (y >= Height)
            {
                Resize(Width, y + 1);
            }

            for (var x = 0; x < Width; x++)
            {
                if (grid[x, y] == null)
                {
                    AddGate(x, y, gate);
                    return;
                }
            }

            AddGate(Width, y, gate);
        }

        /// <summary>Add a gate to the grid.</summary>
        /// <param name="x">Column index.</param>
        /// <param name="y">Row (quit) index.</param>
        /// <param name="gate">The gate to place on the grid.</param>
        public void AddGate(int x, int y, QuantumGate gate)
        {
            if (x < 0 || y < 0)
            {
                throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds of the grid");
            }

            if (x >= Width || y + gate.Height - 1 >= Height)
            {
                Resize(Math.Max(Width, x + 1), Math.Max(Height, y + gate.Height));
            }

            if (grid[x, y] != null)
            {
                throw new ArgumentException($"There already exists a gate at {x}, {y}");
            }

            for (var i = 0; i < gate.Height; i++)
            {
                grid[x, y + i] = gate;
            }
        }

        /// <summary>Shrinks the grid to occupy the least number of squares in each direction.</summary>
        public void Shrink()
        {
            (int maxX, int maxY) = Gates.Aggregate((1, 1), (acc, p) => (Math.Max(acc.Item1, p.X + 1), Math.Max(acc.Item2, p.Y + 1)));
            Resize(maxX, maxY);
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
                    builder.Append(" " + (grid[x, y]?.Name ?? "_"));
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private void Resize(int newWidth, int newHeight)
        {
            var newGrid = new QuantumGate?[newWidth, newHeight];
            var newNames = new string[newHeight];

            for (var y = 0; y < newHeight; y++)
            {
                newNames[y] = y < Height ? Names[y] : $"Q {y}";
                for (var x = 0; x < newWidth; x++)
                {
                    newGrid[x, y] = x < Width && y < Height ? grid[x, y] : null;
                }
            }

            Names = newNames;
            grid = newGrid;
        }
    }
}
