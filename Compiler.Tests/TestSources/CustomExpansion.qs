namespace CustomExpansion {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;
    
    @EntryPoint()
    operation Main() : Unit {
        using (qs = Qubit[2]) {
            Custom(qs);
        }

        using (qs = Qubit[2]) {
            Custom(qs);
        }
    }

    operation Custom(qs: Qubit[]) : Unit {
        H(qs[0]);
        CNOT(qs[0], qs[1]);
        ResetAll(qs);
    }
}