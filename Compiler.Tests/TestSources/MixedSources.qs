namespace MixedSources {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;

    operation Mixed1(q: Qubit, qs: Qubit[]): Unit {
        X(q);       // first
        H(qs[0]);   // second
        Y(qs[3]);   // third
    }

    operation Mixed2(qs: Qubit[], q: Qubit): Unit {
        H(qs[1]);   // second
        X(q);       // first
        Z(qs[2]);   // third
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
