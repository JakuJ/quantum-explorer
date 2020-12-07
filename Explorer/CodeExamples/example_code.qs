namespace HelloWorld {

    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;
    open Microsoft.Quantum.Canon;

    @EntryPoint()
    operation Hello(): Unit {
      using (qs = Qubit[2]) {
            H(qs[0]);
            CNOT(qs[0], qs[1]);
            Controlled Y([qs[0]], qs[1]); 
            Message($"Measured {M(qs[0])}, {M(qs[1])}");
        }
    }
}
