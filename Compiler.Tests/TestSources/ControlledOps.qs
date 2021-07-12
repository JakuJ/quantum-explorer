namespace ControlledOps {
    open Microsoft.Quantum.Intrinsic;
    
    @EntryPoint()
    operation Main(): Unit {
        UsingFunctor();
        CNOTS();
    }
    
    operation UsingFunctor() : Unit {
        use qs = Qubit[3] {
            Controlled X([qs[0]], qs[1]);
            Controlled X([qs[2]], qs[1]);
            Controlled X([qs[1]], qs[0]);
            Controlled X([qs[0], qs[1]], qs[2]);
        }
    }

    operation CNOTS() : Unit {
        use qs = Qubit[3] {
            CNOT(qs[0], qs[1]);
            CNOT(qs[2], qs[1]);
            CNOT(qs[1], qs[0]);
            CCNOT(qs[0], qs[1], qs[2]);
        }
    }

}
