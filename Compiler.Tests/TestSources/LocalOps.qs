namespace LocalOps {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Measurement;

    operation DoesSomethingWithNoQubits() : Result {
        use q = Qubit()
        {
            H(q);
            X(q);
            return MResetZ(q);
        }
    }

    operation DoesSomethingWithASingleQubit(q: Qubit) : Result {
        H(q);
        X(q);
        return MResetZ(q);
    }

    @EntryPoint()
    operation MotherOp(): Unit {
        use q = Qubit()
        {
            let k = DoesSomethingWithNoQubits();
            let x = DoesSomethingWithASingleQubit(q);
        }
    }

}
