namespace MultipleOperations {

    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;

    operation RandomBit () : Result {
        using (q = Qubit()) {
            H(q);
            return MResetZ(q);
        }
    }

    operation Noop(target : Qubit) : Unit {
        // do absolutely nothing
    }

    operation IdentityGate(target : Qubit) : Qubit {
        return target;
    }

    operation EntanglePair(q1 : Qubit, q2 : Qubit) : Unit{
        H(q1);
        Controlled X([q1], q2);
    }
    
}
