namespace CommentedEntryPoint {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;
    
    // haha no @EntryPoint() here
    operation SayHello() : Unit {
        using (q = Qubit()) {
            H(q);
            let result = MResetZ(q);
            Message($"Result is {result} but this should not execute");
        }
    }
}
