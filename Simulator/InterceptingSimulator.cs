using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using Simulator.Custom;

namespace Simulator
{
    /// <inheritdoc />
    public class InterceptingSimulator : QuantumSimulator
    {
        private static readonly Regex[] ExpandedOps = new[]
        {
            @"Microsoft\.Quantum\.Canon\..+",
            @"Microsoft\.Quantum\.Arrays\..+",
            @"Microsoft\.Quantum\.Arithmetic\..+",
            @"Microsoft\.Quantum\.Measurement\..+",
            @"Microsoft\.Quantum\.Intrinsic\.C?CNOT",
            @"Microsoft\.Quantum\.Intrinsic\.Reset(All)?",
        }.Select(x => new Regex(x)).ToArray();

        private readonly bool expanding;

        private readonly ImmutableHashSet<string> userNamespaces;

        private readonly StringBuilder funnel = new();

        private readonly List<(string, bool)> operationStack = new();

        private readonly Dictionary<string, List<GateGrid>> gateGrids = new();

        private readonly Dictionary<int, string> qubitIds = new();

        private readonly Queue<int[]> allocationQueue = new();

        /// <inheritdoc cref="QuantumSimulator"/>
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptingSimulator" /> class.
        /// </summary>
        public InterceptingSimulator(IEnumerable<string> userNamespaces, bool expanding) : base(false)
        {
            this.expanding = expanding;
            this.userNamespaces = userNamespaces.ToImmutableHashSet();
            OnOperationStart += OperationStartHandler;
            OnOperationEnd += OperationEndHandler;
            AfterAllocateQubits += AllocateQubitsHandler;
        }

        /// <summary>
        /// Gets the messages intercepted during simulation.
        /// </summary>
        public string Messages => funnel.ToString();

        /// <summary>
        /// Gets the GateGrids constructed by tracing operation applications in this simulator.
        /// </summary>
        /// <returns>A dictionary mapping operation names to a list of gate grids.</returns>
        public Dictionary<string, List<GateGrid>> GetGrids()
        {
            Dictionary<string, List<GateGrid>> ret = new();

            foreach ((string op, List<GateGrid> gridList) in gateGrids)
            {
                ret.Add(op, gridList.ToHashSet().ToList());
            }

            return ret;
        }

        private void AllocateQubitsHandler(IQArray<Qubit> qubits) => allocationQueue.Enqueue(qubits.Select(x => x.Id).ToArray());

        private void OperationStartHandler(ICallable op, IApplyData data)
        {
            int[] qubits = (data.Qubits?.Select(x => x.Id) ?? Enumerable.Empty<int>()).ToArray();
            int[]? controls = null;
            string @namespace = op.FullName[..^(op.Name.Length + 1)];

            Console.WriteLine($"\n=====\nOperation is {op.FullName}");

            // Get runtime information about this operation application
            RuntimeMetadata? metadata = op.GetRuntimeMetadata(data);
            if (metadata != null)
            {
                // Check for control qubits
                if (metadata.IsControlled)
                {
                    controls = metadata.Controls
                                       .Select(x => x.Id)
                                       .ToArray();
                }
            }

            // Check if this operation is phantom
            bool isCustom = userNamespaces.Contains(@namespace);
            bool isPhantom = ExpandedOps.Any(x => x.Match(op.FullName).Success) || (expanding && isCustom);

            // If it's not the entry-point operation and it takes qubit arguments
            if (!isPhantom && operationStack.Count > 0 && qubits.Length > 0)
            {
                GateGrid[] gridsToAdd = Array.Empty<GateGrid>();

                if (!expanding)
                {
                    // Find first non-phantom parent
                    int i = operationStack.Count - 1;
                    (string parentOperation, bool isParentPhantom) = operationStack[i];
                    while (!expanding && isParentPhantom)
                    {
                        (parentOperation, isParentPhantom) = operationStack[--i];
                    }

                    // Only add gates to the parent grid
                    if (gateGrids.TryGetValue(parentOperation, out var grids))
                    {
                        gridsToAdd = new[] { grids.Last() };
                    }
                }
                else
                {
                    // Add gates to all grids on the stack
                    gridsToAdd = operationStack
                                .FindAll(x => gateGrids.ContainsKey(x.Item1))
                                .Select(x => gateGrids[x.Item1].Last())
                                .ToArray();
                }

                // Add gates to grid[s]
                foreach (var grid in gridsToAdd)
                {
                    // Check if we can place this gate set in the last column
                    var lastColumn = Enumerable
                                    .Range(0, grid.Height)
                                    .Where(r => grid.At(grid.Width - 1, r) != null)
                                    .ToHashSet();
                    bool sharesQubits = qubits.ToHashSet().Overlaps(lastColumn);

                    bool lastColumnHasCtl = Enumerable
                                           .Range(0, grid.Height)
                                           .Any(r => grid.At(grid.Width - 1, r)?.Name == "__control__");

                    bool lastColumnHasOther = Enumerable
                                             .Range(0, grid.Height)
                                             .Select(r => grid.At(grid.Width - 1, r))
                                             .Where(x => x.HasValue)
                                             .Any(x => x.Value.FullName != op.FullName);

                    if (sharesQubits
                     || lastColumnHasCtl
                     || (metadata?.IsControlled ?? false)
                     || lastColumnHasOther)
                    {
                        Console.WriteLine($"Shares qubits, adding column at {grid.Width}");
                        grid.InsertColumn(grid.Width);
                    }

                    // Index of last column
                    int x = Math.Max(0, grid.Width - 1);

                    foreach ((int argIndex, int qubitID) in qubits.Enumerate())
                    {
                        // If a name was already set on the qubit, the ID itself might have been
                        // changed in the meantime due to qubit re-allocations
                        int idx = grid.Names.IndexOf(qubitIds[qubitID]);
                        int qubit = idx != -1 ? idx : qubitID;
                        int k = x;

                        // If a qubit occurs more than one time, move subsequent gates to the right
                        while (grid.At(k, qubit) != null)
                        {
                            k++;
                        }

                        if (controls != null && Array.IndexOf(controls, qubitID) >= 0)
                        {
                            // Create custom gates for control qubits
                            grid.AddGate(k, qubit, CustomGateFactory.MakeCustomGate("__control__"));
                            Console.WriteLine($"Adding control to grid at {k}, {qubit}");
                        }
                        else
                        {
                            // And normal gates for intrinsics / custom operations
                            grid.AddGate(k, qubit, new QuantumGate(op.Name, @namespace, argIndex));
                            Console.WriteLine($"Adding {op.Name} to grid at {k}, {qubit}");
                        }

                        // Set qubit identifier if not present
                        if (grid.Names[qubit] == null)
                        {
                            Console.WriteLine($"Setting name {qubitIds[qubit]} to {qubit}");
                            grid.SetName(qubit, qubitIds[qubit]);
                        }
                    }
                }
            }

            // Push current operation onto the call stack
            operationStack.Add((op.FullName, isPhantom));

            if (!userNamespaces.Contains(@namespace))
            {
                return;
            }

            // Prepare an empty gate grid [list]
            if (!gateGrids.ContainsKey(op.FullName))
            {
                gateGrids.Add(op.FullName, new List<GateGrid>());
            }

            Console.WriteLine($"Adding grid for operation {op.FullName}");
            gateGrids[op.FullName].Add(new GateGrid());
        }

        private void OperationEndHandler(ICallable op, IApplyData data)
        {
            Console.WriteLine($"{op.FullName} ending");

            List<GateGrid>? grids = gateGrids.GetValueOrDefault(operationStack.Last().Item1);
            GateGrid? last = grids?.Last();

            if (last != null)
            {
                if (last.Height > 0)
                {
                    // Remove unnecessary qubits
                    last.RemoveEmptyRows();
                    last.SortRowsByQubitIds();
                }
                else
                {
                    // Remove empty grids
                    grids!.RemoveAt(grids.Count - 1);
                }
            }

            operationStack.RemoveAt(operationStack.Count - 1);
        }

        /// <summary>A custom intrinsic operation for runtime allocation tagging.</summary>
        public class TagAllocationImpl : TagAllocation
        {
            private readonly InterceptingSimulator sim;

            /// <summary>
            /// Initializes a new instance of the <see cref="TagAllocationImpl"/> class.
            /// </summary>
            /// <param name="m">The simulator owning this implementation.</param>
            public TagAllocationImpl(InterceptingSimulator m) : base(m) => sim = m;

// Tuple types in signatures should have element names
#pragma warning disable SA1414
            /// <inheritdoc cref="TagAllocation" />
            public override Func<(string, bool), QVoid> __Body__
#pragma warning restore SA1414
            {
                get
                {
                    return args =>
                    {
                        // Q#: using (q1, qs, q2) = (Qubit(), Qubit[3 + n], Qubit())
                        // Calls to Allocate: Allocate(1), Allocate(3 + n), Allocate(1)
                        // Calls to TagAllocation: Tag(a, false), Tag(b, true), Tag(c, false)
                        (var id, bool isRegister) = args;
                        int[] ids = sim.allocationQueue.Dequeue();

                        if (isRegister)
                        {
                            for (var i = 0; i < ids.Length; i++)
                            {
                                sim.qubitIds[ids[i]] = $"{id}[{i}]";
                                Console.WriteLine($"Qubit {ids[i]} is {id}[{i}]");
                            }
                        }
                        else
                        {
                            sim.qubitIds[ids[0]] = id;
                            Console.WriteLine($"Qubit {ids[0]} is {id}");
                        }

                        return QVoid.Instance;
                    };
                }
            }
        }

        /// <summary>The overriding definition for the Message operation.</summary>
        public new class Message : Microsoft.Quantum.Intrinsic.Message
        {
            private readonly InterceptingSimulator sim;

            /// <summary>
            /// Initializes a new instance of the <see cref="Message"/> class.
            /// </summary>
            /// <param name="m">The simulator owning this implementation.</param>
            public Message(InterceptingSimulator m) : base(m) => sim = m;

            /// <inheritdoc/>
            public override Func<string, QVoid> __Body__
            {
                get
                {
                    return msg =>
                    {
                        sim.funnel.AppendLine(msg);
                        return QVoid.Instance;
                    };
                }
            }
        }
    }
}
