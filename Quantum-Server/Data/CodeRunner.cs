using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Quantum.Simulation.Common;
using Microsoft.Quantum.Simulation.Simulators;
using Quantum_Server.Extensions;
using Quantum.Immediate;

namespace Quantum_Server.Data
{
    public static class CodeRunner
    {
        private static readonly QuantumOp<string, long> FromText = async (qsim, arg) =>
        {
            var gates = new List<Gate>();

            foreach (char c in arg)
            {
                gates.Add(await ParseImmediate.Run(qsim, c.ToString()));
            }

            return await RunGates.Run(qsim, gates.ToQArray());
        };

        public static async Task<string> RunCode(string code)
        {
            using var qsim = new QuantumSimulator();
            long res = await FromText(qsim, code);
            return res.ToString();
        }

        private delegate Task<TRes> QuantumOp<in T, TRes>(SimulatorBase qsim, T arg);
    }
}
