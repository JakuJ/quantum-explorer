using System;
using System.Collections.Generic;
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
        private readonly bool skipIntrinsic;
        private readonly StringBuilder funnel = new();
        private readonly Stack<string> currentOperation = new();
        private readonly Dictionary<string, List<GateGrid>> grids = new();
        private readonly Dictionary<int, string> qubitIds = new();
        private readonly Queue<int[]> allocationQueue = new();

        /// <inheritdoc cref="QuantumSimulator"/>
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptingSimulator" /> class.
        /// </summary>
        public InterceptingSimulator(bool skipIntrinsic = true) : base(false)
        {
            this.skipIntrinsic = skipIntrinsic;
            OnOperationStart += CountOperationCalls;
            OnOperationEnd += EndOperationCallHandler;
            AfterAllocateQubits += OnAllocate;
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

            foreach ((string op, List<GateGrid> gridList) in grids)
            {
                ret.Add(op, gridList.ToHashSet().ToList());
            }

            return ret;
        }

        private void OnAllocate(IQArray<Qubit> qubits)
        {
            allocationQueue.Enqueue(qubits.Select(x => x.Id).ToArray());

            // TODO: Remove debug print statements
            Console.Write($"Allocated {qubits.Count} qubits:");
            foreach (var qubit in qubits)
            {
                Console.Write($" {qubit.Id}");
            }

            Console.WriteLine();
        }

        private void CountOperationCalls(ICallable op, IApplyData data)
        {
            // Get qubits affected by this operation
            Qubit[]? qubits = data.Qubits?.ToArray();

            if (qubits != null && this.grids.TryGetValue(currentOperation.Peek(), out var grids))
            {
                GateGrid grid = grids.Last();
                int x = grid.Width;

                foreach ((int index, var qubit) in qubits.Enumerate())
                {
                    grid.AddGate(x, qubit.Id, new QuantumGate(op.Name, op.FullName[..^(op.Name.Length + 1)], index));
                    grid.SetName(qubit.Id, qubitIds[qubit.Id]);
                }
            }

            EnterOperation(op.FullName);
        }

        private void EndOperationCallHandler(ICallable op, IApplyData data)
        {
            // Remove unnecessary qubits
            // TODO: Sort rows by qubit ID
            grids.GetValueOrDefault(currentOperation.Peek())?.Last().RemoveEmptyRows();

            string pop = currentOperation.Pop();
            Console.WriteLine($"Popping {pop} off the stack");
        }

        private void EnterOperation(string op)
        {
            // Set current operation
            currentOperation.Push(op);
            Console.WriteLine($"Pushing {op} onto stack");

            if (op.StartsWith("Simulator.Utils") || (skipIntrinsic && op.StartsWith("Microsoft.Quantum")))
            {
                return;
            }

            if (!grids.ContainsKey(op))
            {
                grids.Add(op, new List<GateGrid>());
            }

            grids[op].Add(new GateGrid());
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
                                Console.WriteLine($"Assigning ID {sim.qubitIds[ids[i]]} to qubit {ids[i]}");
                            }
                        }
                        else
                        {
                            sim.qubitIds[ids[0]] = id;
                            Console.WriteLine($"Assigning ID {id} to qubit {ids[0]}");
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
