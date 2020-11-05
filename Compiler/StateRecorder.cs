using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using static Microsoft.Quantum.Simulation.Simulators.QuantumSimulator;

namespace Compiler
{
    /// <summary>
    /// Records quantum states for simulation.
    /// </summary>
    public class StateRecorder
    {
        private InterceptingSimulator simulator;
        private CustomDumper dumper;

        private OperationState currentOperation;

        /// <summary>
        /// Gets a root operation of the simulated program.
        /// </summary>
        public OperationState Root { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateRecorder"/> class.
        /// </summary>
        /// <param name="qSimulator">Simulator running the simulation.</param>
        public StateRecorder(InterceptingSimulator qSimulator)
        {
            simulator = qSimulator;
            dumper = new CustomDumper(simulator);

            Root = new OperationState("");
            currentOperation = Root;

            simulator.OnOperationStart += OnOperationStartHandler;
            simulator.OnOperationEnd += OnOperationEndHandler;
        }

        private void OnOperationStartHandler(ICallable operation, IApplyData input)
        {
            var opState = new OperationState(operation.Name);
            currentOperation.AddOperation(opState);
            currentOperation = opState;
            var qubits = input.Qubits?.Select(q => q.Id).ToArray() ?? Array.Empty<int>();
            dumper.Dump();
            currentOperation.Arguments = dumper.Values;
        }

        private void OnOperationEndHandler(ICallable operation, IApplyData output)
        {
            dumper.Dump();
            currentOperation.Results = dumper.Values;
            currentOperation = currentOperation.Parent;
        }

    }

}
