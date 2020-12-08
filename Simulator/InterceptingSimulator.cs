using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Common;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using Simulator.Utils;

namespace Simulator
{
    /// <inheritdoc />
    public class InterceptingSimulator : QuantumSimulator
    {
        private static readonly ImmutableHashSet<string> ExcludedNamespaces
            = ImmutableHashSet.Create("Simulator.Utils", "__custom__");

        private readonly StringBuilder funnel = new();

        private readonly Stack<string> currentOperation = new();

        private readonly Dictionary<string, List<GateGrid>> gateGrids = new();

        private readonly Dictionary<int, string> qubitIds = new();

        private readonly Queue<int[]> allocationQueue = new();

        /// <inheritdoc cref="QuantumSimulator"/>
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptingSimulator" /> class.
        /// </summary>
        public InterceptingSimulator() : base(false)
        {
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

            // Add corresponding column(s)
            if (qubits.Length > 0 && gateGrids.TryGetValue(currentOperation.Peek(), out var grids))
            {
                GateGrid grid = grids.Last();
                int x = grid.Width;

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
                        switch (op.FullName)
                        {
                            case "Microsoft.Quantum.Intrinsic.CNOT":
                            case "Microsoft.Quantum.Intrinsic.CCNOT":
                                grid.AddGate(k, qubit, new QuantumGate("X", @namespace, 0));
                                break;
                            default:
                                grid.AddGate(k, qubit, new QuantumGate(op.Name, @namespace, argIndex));
                                break;
                        }
                    }

                    // Set qubit identifier
                    grid.SetName(qubit, qubitIds[qubit]);
                }
            }

            PushOperation(op.FullName, @namespace);
        }

        private void OperationEndHandler(ICallable op, IApplyData data)
        {
            // Remove unnecessary qubits
            GateGrid? last = gateGrids.GetValueOrDefault(currentOperation.Peek())?.Last();
            last?.RemoveEmptyRows();
            last?.SortRowsByQubitIds();

            currentOperation.Pop();
        }

        private void PushOperation(string fullName, string @namespace)
        {
            // Set current operation
            currentOperation.Push(fullName);

            if (fullName.StartsWith("Microsoft.Quantum") || ExcludedNamespaces.Contains(@namespace))
            {
                return;
            }

            if (!gateGrids.ContainsKey(fullName))
            {
                gateGrids.Add(fullName, new List<GateGrid>());
            }

            gateGrids[fullName].Add(new GateGrid());
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

            /// <inheritdoc/>
            public override Func<(string, bool), QVoid> __Body__
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
