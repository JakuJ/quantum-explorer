namespace MixedSources {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;

    @EntryPoint()
    operation Main(): Unit {
        use qs = Qubit[6] {
            Mixed1(qs[0], qs[1..5]);
            Mixed2(qs[1..5], qs[0]);
            Mixed3(qs[0..2], qs[3..5]);
        }
    }

    operation Mixed1(q: Qubit, qs: Qubit[]): Unit {
        X(q);       // first
        H(qs[0]);   // second
        Y(qs[3]);   // third
    }

    operation Mixed2(qs: Qubit[], q: Qubit): Unit {
        H(qs[1]);   // first
        X(q);       // third
        Z(qs[2]);   // second
    }

    operation Mixed3(reg1: Qubit[], reg2: Qubit[]): Unit {
        X(reg1[0]); // 0
        Y(reg2[0]); // 3
        
        X(reg1[1]); // 1
        Y(reg2[1]); // 4
        
        X(reg1[2]); // 2
        Y(reg2[2]); // 5
    }

}
