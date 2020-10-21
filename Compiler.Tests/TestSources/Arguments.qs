namespace Arguments {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;

    // helper operations
    operation TwoArg(q1: Qubit, q2: Qubit): Unit {}
    operation RegArg(qs: Qubit[]): Unit {}
    operation SingleAndRegArgs(q1: Qubit, qs: Qubit[], q2: Qubit): Unit {}

    // test operation
    operation MotherOp(): Unit {
        using (qs = Qubit[5]) {
            X(qs[0]);                           
            TwoArg(qs[0], qs[1]);               
            TwoArg(qs[4], qs[4]);               
            RegArg(qs);                         
            RegArg([qs[1], qs[0], qs[3]]);      
            SingleAndRegArgs(qs[4], qs, qs[1]);
        }
    }
}

// X_13_0 TwoArg_14_0     _            _       RegArg_16_0A RegArg_17_1
// _      TwoArg_14_1     _            _       RegArg_16_0A RegArg_17_0
// _         _            _            _       RegArg_16_0A    _
// _         _            _            _       RegArg_16_0A RegArg_17_2
// _         _         TwoArg_15_0 TwoArg_15_0 RegArg_16_0A    _
