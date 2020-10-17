namespace LocalOps {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Measurement;

    operation DoesSomethingWithNoQubits() : Result {
        using (q = Qubit())
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

    operation MotherOp(): Unit {
        using (q = Qubit())
        {
            let k = DoesSomethingWithNoQubits();
            let x = DoesSomethingWithASingleQubit(q);
        }
    }

}
