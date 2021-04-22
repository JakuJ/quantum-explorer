namespace MultipleOperations {

    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;

    @EntryPoint()
    operation Main(): Unit {
        let x = RandomBit();
        use (q1, q2) = (Qubit(), Qubit()) {
            NoOp();
            let q11 = IdentityGate(q1);
            let q22 = IdentityGate(q2);
            EntanglePair(q1, q2);
            ResetAll([q1, q2]);
        }
    }

    operation RandomBit () : Result {
        use q = Qubit() {
            H(q);
            return MResetZ(q);
        }
    }

    operation NoOp() : Unit {
        // do absolutely nothing
    }

    operation IdentityGate(target : Qubit) : Qubit {
        return target;
    }

    operation EntanglePair(q1 : Qubit, q2 : Qubit) : Unit {
        H(q1);
        Controlled X([q1], q2);
    }
    
}
