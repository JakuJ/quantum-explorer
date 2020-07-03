namespace Quantum_Server.Data.Intermediate
{
    public interface IGate
    {
        public int     Qubits   { get; }
        public IGate[] Controls { get; set; }
        public string  Symbol   { get; }
    }

    public class HGate : IGate
    {
        public int     Qubits   => 1;
        public IGate[] Controls { get; set; }
        public string  Symbol   => "H";

        public HGate() {}
        public HGate(IGate[] controls) => Controls = controls;
    }
    
    public class XGate : IGate
    {
        public int     Qubits   => 1;
        public IGate[] Controls { get; set; }
        public string  Symbol   => "X";

        public XGate() {}
        public XGate(IGate[] controls) => Controls = controls;
    }
    
    public class NoOpGate : IGate
    {
        public int     Qubits   => 1;
        public IGate[] Controls { get; set; }
        public string  Symbol   => "I";

        public NoOpGate() {}
        public NoOpGate(IGate[] controls) => Controls = controls;
    }
}