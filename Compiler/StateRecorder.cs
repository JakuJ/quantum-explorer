using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Compiler
{
    /// <summary>
    /// Records quantum states during a simulation.
    /// </summary>
    public class StateRecorder
    {
        private readonly CustomDumper dumper;

        private OperationState currentOperation;

        /// <summary>Initializes a new instance of the <see cref="StateRecorder"/> class.</summary>
        /// <param name="simulator">Simulator running the simulation.</param>
        public StateRecorder(QuantumSimulator simulator)
        {
            dumper = new CustomDumper(simulator);

            Root = new OperationState("");
            currentOperation = Root;

            simulator.OnOperationStart += OnOperationStartHandler;
            simulator.OnOperationEnd += OnOperationEndHandler;
        }

        /// <summary>Gets the root operation of the simulated program.</summary>
        public OperationState Root { get; }

        private void OnOperationStartHandler(ICallable operation, IApplyData input)
        {
            var opState = new OperationState(operation.Name);
            currentOperation.AddOperation(opState);
            currentOperation = opState;
            dumper.Dump();
            currentOperation.Arguments = dumper.Values;
        }

        private void OnOperationEndHandler(ICallable operation, IApplyData output)
        {
            dumper.Dump();
            currentOperation.Results = dumper.Values;
            currentOperation = currentOperation.Parent!; // an operation that's ending always has a parent operation
        }
    }
}
