namespace Microsoft.Quantum.TestSample {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;

    @EntryPoint()
    operation Main() : Unit {
        let x = RandomBit();
    }

    operation RandomBit () : Result {
        use q = Qubit() {
            H(q);
            return MResetZ(q);
        }
    }

}
