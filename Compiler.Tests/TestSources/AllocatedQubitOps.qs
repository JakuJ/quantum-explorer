namespace AllocatedQubitOps {
    
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;

    @EntryPoint()
    operation Main(): Unit {
        AllocateOne();
        let x = AllocateOneAndApplyGates();
        AllocateFive();
        AllocateFiveAndApplyGates();
        Tuples();
    }

    // Single qubit, no gates
    operation AllocateOne() : Unit {
        using (qubit = Qubit()) {}
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

    // Complex using statement
    operation Tuples(): Unit {
        using (((q1, qs), q2) = ((Qubit(), Qubit[1]), Qubit())) {
            X(q1); // 1st row
            Y(qs[0]); // 3rd row
            H(q2); // 2nd row
        }
    }
}