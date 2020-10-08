using System.Collections.Generic;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class GateGridTest
    {
        private static class Sources
        {
            public static IEnumerable<GateGrid> Cases
            {
                get
                {
                    yield return new GateGrid(3, 1);
                    yield return new GateGrid(1, 3);
                    yield return new GateGrid(5, 5);

                    var grid = new GateGrid(1, 1);
                    grid.AddGate(0, new QuantumGate("H"));
                    grid.AddGate(0, new QuantumGate("X"));
                    grid.AddGate(0, new QuantumGate("Z"));
                    grid.AddGate(1, new QuantumGate("MResetZ"));
                    grid.AddGate(1, new QuantumGate("UnknownOperation"));

                    yield return grid;
                }
            }
        }
    }
}
