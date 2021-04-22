namespace Borrowing {

    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;

    // Test "borrowing" statement and also partial functions 
    @EntryPoint()
    operation Main(): Unit {
        borrow qs = Qubit[3] {
            ApplyToEachCA(Ry(0.5, _), qs);
            ResetAll(qs);
        }
    }
}