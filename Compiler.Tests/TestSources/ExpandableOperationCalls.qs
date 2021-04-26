namespace ExpandableOperationCalls {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Arrays;

    @EntryPoint()
    operation RunProgram() : Unit {
        use qs = Qubit[5] {
            CX(qs[0], qs[1]);
            CY(qs[1], qs[2]);
            CZ(qs[0], qs[2]);
            CNOT(qs[1], qs[2]);
            CCNOT(qs[0], qs[1], qs[2]);
            AndLadder(CCNOTop(CCNOT), [qs[0], qs[1], qs[2]], [qs[3], qs[4]]);
            ApplyCNOTChainWithTarget(qs[...3], qs[4]);
            ApplyToEachCA(I, qs);
            ResetAll(qs);
        }
    }

}
