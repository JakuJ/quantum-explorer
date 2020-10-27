using System;
using System.Text;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Compiler
{
    /// <summary>
    /// A simulator which extends QuantumSimulator and redefines the Message operation
    /// to intercept text output from the simulator.
    /// </summary>
    public class InterceptingSimulator : QuantumSimulator
    {
        private readonly StringBuilder funnel = new StringBuilder();

        /// <summary>
        /// Gets the messages intercepted during simulation.
        /// </summary>
        public string Messages => funnel.ToString();

        /// <summary>
        /// The overriding definition for operation Message.
        /// </summary>
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
