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
            @"Microsoft\.Quantum\.Intrinsic\.C?CNOT",
        }.Select(x => new Regex(x)).ToArray();

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
        public InterceptingSimulator(IEnumerable<string> userNamespaces) : base(false)
        {
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
            bool isPhantom = ExpandedOps.Any(x => x.Match(op.FullName).Success);

            // If it's not the entry-point operation
            if (operationStack.Count > 0)
            {
                // Find first non-phantom parent
                int i = operationStack.Count - 1;
                (string parentOperation, bool isParentPhantom) = operationStack[i];
                while (isParentPhantom)
                {
                    (parentOperation, isParentPhantom) = operationStack[--i];
                }

                // If we had a phantom parent, we would like to place this call in the last column
                // and not add another one.
                bool hasPhantomParent = i != operationStack.Count - 1;

                // Add corresponding column(s)
                if (qubits.Length > 0 && gateGrids.TryGetValue(parentOperation, out var grids))
                {
                    GateGrid grid = grids.Last();
                    if (isPhantom)
                    {
                        // Do not insert an empty column at the end if one was already inserted
                        // This is a workaround for nested phantom operations
                        if (grid.Height == 0 || Enumerable.Range(0, grid.Height).Any(r => grid.At(grid.Width - 1, r) != null))
                        {
                            grid.InsertColumn(grid.Width);
                        }
                    }
                    else
                    {
                        int x = grid.Width - (hasPhantomParent ? 1 : 0);

                        foreach ((int argIndex, int qubit) in qubits.Enumerate())
                        {
                            // If a qubit occurs more than one time, move subsequent gates to the right
                            int k = x;
                            while (grid.At(k, qubit) != null)
                            {
                                k++;
                            }

                            // Create custom gates for control qubits
                            if (controls != null && Array.IndexOf(controls, qubit) >= 0)
                            {
                                grid.AddGate(k, qubit, CustomGateFactory.MakeCustomGate("__control__"));
                            }
                            else
                            {
                                grid.AddGate(k, qubit, new QuantumGate(op.Name, @namespace, argIndex));
                            }

                            // Set qubit identifier
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

            gateGrids[op.FullName].Add(new GateGrid());
        }

        private void OperationEndHandler(ICallable op, IApplyData data)
        {
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
                            }
                        }
                        else
                        {
                            sim.qubitIds[ids[0]] = id;
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
