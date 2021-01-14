namespace HelloWorld {

    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;

    operation IgnoreThis(): Unit {
        Message("This should not be printed!");
    }

    operation DoNothing1(): Unit {}
    
    @EntryPoint()
    operation HelloQ() : Unit {
        DoNothing1();
        Message("Hello World!");
        DoNothing2();
    }

    operation DoNothing2(): Unit {}

    operation IgnoreThisAsWell(): Unit {
        Message("This should not be printed as well!");
    }

}