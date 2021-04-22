namespace UseAfterRelease {
    open Microsoft.Quantum.Intrinsic;

    operation BadAlloc(): Qubit {
        use q = Qubit() {
            X(q);               // legal Q#
            return q;
        }
    }

    @EntryPoint()
    operation Main(): Unit {
        let q = BadAlloc();
        H(q);                   // forbidden Q#
        Reset(q);
    }

}