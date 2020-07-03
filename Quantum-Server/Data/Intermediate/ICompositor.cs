using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantum_Server.Data.Intermediate
{
    public interface ICompositor
    {
        public int Qubits { get; set; }
        public List<(int, IGate)> QubitGates(int row);

        public void AddGate(int qubit, int column, IGate gate);
    }

    public class TextCompositor : ICompositor
    {
        private readonly StringBuilder _builder;
        
        public TextCompositor(StringBuilder builder)
        {
            _builder = builder;
            Code = "";
        }

        public string Code
        {
            get => _builder.ToString();
            set
            {
                _builder.Clear();
                _builder.Append(value);
            }
        }

        public int Qubits
        {
            get => 1;
            set => throw new NotImplementedException();
        }

        public List<(int, IGate)> QubitGates(int row)
        {
            if (row != 0)
            {
                throw new ArgumentException("There is only one qubit!");
            }

            return Code.Select((c, i) => (i, GateFromChar(c))).ToList();
        }

        private IGate GateFromChar(in char c)
        {
            return c switch
            {
                'H' => new HGate(),
                'X' => new XGate(),
                _   => new NoOpGate()
            };
        }


        public void AddGate(int qubit, int column, IGate gate)
        {
            _builder[column] = gate.Symbol[0];
        }
    }
}