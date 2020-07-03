namespace Quantum.Immediate {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Measurement;

    newtype Gate = (Apply: (Qubit => Unit is Adj + Ctl));
    
    function ParseImmediate(repr: String) : Gate {

        if (repr == "H") {
            return Gate(H);
        } elif (repr == "X") {
            return Gate(X);
        }
        return Gate(I);
    }

    operation RunGates(gates : Gate[]) : Int {
        using (q = Qubit()) {
            for (gate in gates) {
                gate::Apply(q);
            }
            return MResetZ(q) == One ? 1 | 0;
        }
    }
}
