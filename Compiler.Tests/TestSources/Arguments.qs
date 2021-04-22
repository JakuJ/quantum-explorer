namespace Arguments {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;

    // helper operations
    operation TwoArgs(q1: Qubit, q2: Qubit): Unit {}
    operation RegArg(qs: Qubit[], k: Int): Unit {}
    operation SingleAndRegArgs(q1: Qubit, qs: Qubit[], q2: Qubit): Unit {}

    operation Ignored(num: Int, nums: Int[]): Unit {}

    @EntryPoint()
    operation TestOp(): Unit {
        use qs = Qubit[5] {
            X(qs[0]);
            TwoArgs(qs[0], qs[1]);
            TwoArgs(qs[4], qs[4]);
            RegArg([qs[1], qs[0], qs[3]], 1337); // Int argument should be ignored
            SingleAndRegArgs(qs[4], [qs[2], qs[3]], qs[1]);

            Ignored(5, [4, 2, 0]);
            
            let xs = [2, 1, 3, 7];
            Ignored(456, xs);
        }
    }
}
