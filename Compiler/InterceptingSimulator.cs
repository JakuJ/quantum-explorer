using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Compiler
{
    /// <inheritdoc />
    public class InterceptingSimulator : QuantumSimulator
    {
        private readonly bool skipIntrinsic;
        private readonly StringBuilder funnel = new();
        private readonly Stack<string> currentOperation = new();

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
        /// Gets the GateGrids constructed by tracing operation applications in this simulator.
        /// </summary>
        public Dictionary<string, List<GateGrid>> Grids { get; } = new();

        /// <summary>
        /// Gets the messages intercepted during simulation.
        /// </summary>
        public string Messages => funnel.ToString();

        private static void OnAllocate(IQArray<Qubit> qubits)
        {
            foreach (var qubit in qubits)
            {
                Console.WriteLine(qubit.Id);
            }
        }

        private void CountOperationCalls(ICallable op, IApplyData data)
        {
            // Get qubits affected by this operation
            Qubit[]? qubits = data.Qubits?.ToArray();

            if (qubits != null && Grids.TryGetValue(currentOperation.Peek(), out var grids))
            {
                GateGrid grid = grids.Last();
                int x = grid.Width;

                foreach ((int index, var qubit) in qubits.Enumerate())
                {
                    grid.AddGate(x, qubit.Id, new QuantumGate(op.Name, op.FullName[..^(op.Name.Length + 1)], index));
                    grid.SetName(qubit.Id, $"Q{qubit.Id}");
                }
            }

            EnterOperation(op.FullName);
        }

        private void EndOperationCallHandler(ICallable op, IApplyData data)
        {
            // Remove unnecessary qubits
            Grids.GetValueOrDefault(currentOperation.Peek())?.Last().RemoveEmptyRows();

            currentOperation.Pop();
        }

        private void EnterOperation(string op)
        {
            // Set current operation
            currentOperation.Push(op);

            if (skipIntrinsic && op.StartsWith("Microsoft.Quantum"))
            {
                return;
            }

            if (!Grids.ContainsKey(op))
            {
                Grids.Add(op, new List<GateGrid>());
            }

            Grids[op].Add(new GateGrid());
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
