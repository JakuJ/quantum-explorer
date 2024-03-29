using System;
using System.Collections.Generic;
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

        private readonly Dictionary<OperationState, OperationState> parents = new();

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
            if (operation.FullName == "Simulator.Custom.TagAllocation")
            {
                return;
            }

            var opState = new OperationState(operation.Name);

            currentOperation.Children.Add(opState);
            parents[opState] = currentOperation;

            currentOperation = opState;
            dumper.Dump();
            currentOperation.Arguments = dumper.Values;
        }

        private void OnOperationEndHandler(ICallable operation, IApplyData output)
        {
            if (operation.FullName == "Simulator.Custom.TagAllocation")
            {
                return;
            }

            dumper.Dump();
            currentOperation.Results = dumper.Values;

            // keys can be missing from the dictionary if the simulation gets interrupted
            // like whenever use-after-release happens
            if (parents.ContainsKey(currentOperation))
            {
                currentOperation = parents[currentOperation];
            }
        }
    }
}
