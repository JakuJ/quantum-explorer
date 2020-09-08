namespace Allocations {
    
    // Single qubit, no gates
    operation AllocateOne() : Unit {
        using (q = Qubit()) {}
    }
    
    // Single qubit, one gate
    operation AllocateOneAndApplyH() : Unit {
        using (q = Qubit()) {
            H(q);
        }
    }
    
    // Single qubit, two gates, returned value
    operation AllocateOneAndApplyGates() : Result {
        using (q = Qubit()) {
            H(q);
            return MResetZ(q);
        }
    }

    // Two qubits, no gates
    operation AllocateTwo() : Unit {
        using (qs = Qubit[2]) {}
    }
    
    // Three qubits, five gates
    operation QuantumTeleportation() : Result {
        using (qs = Qubit[3]) {
            // Entangle qubit 1 and qubit 2
            H(qs[1]);
            Controlled X(qs[1], qs[2]);

            // Message is just |1>
            X(qs[0]);
            
            // Entangle qubit 0 and qubit 1
            Controlled X(qs[0], qs[1]);

            // Hadamard qubit 0
            H(qs[0]);
            
            // Make measurements
            let m1 = MResetZ(qs[0]);
            let m2 = MResetZ(qs[1]);

            // Decode the message
            Controlled X(m1, qs[2]);
            Controlled Z(m0, qs[2]);

            // Should be One
            return MResetZ(qs[2]);
        }
    }
}