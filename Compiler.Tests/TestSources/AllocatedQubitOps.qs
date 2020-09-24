namespace Allocations {
    
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;

    // Single qubit, no gates
    operation AllocateOne() : Unit {
        using (q = Qubit()) {}
    }
    
    // Single qubit, three gates
    operation AllocateOneAndApplyGates() : Result {
        using (q = Qubit()) {
            H(q);
            Z(q);
            X(q);
            return MResetZ(q);
        }
    }

    // A register, no gates
    operation AllocateFive() : Unit {
        using (qs = Qubit[5]) {}
    }

    // A register, gates on different qubits
    operation AllocateFiveAndApplyGates(): Unit {
        using (qs = Qubit[5]) {
            H(qs[0]);
            
            X(qs[1]);
            Y(qs[1]);

            Z(qs[4]);
        }
    }
}