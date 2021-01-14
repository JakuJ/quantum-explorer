// namespace AllocationTagging1 {

// namespace
//  AllocationTagging2 {

// namespace
// AllocationTagging3 
// {

namespace AllocationTagging4{

    open Microsoft.Quantum.Intrinsic;
    
    @EntryPoint()
    operation Main(): Unit {
        DoStuff();
        AllocationTagging5.DoStuff();
        AllocationTagging6.DoStuff();
        AllocationTagging7.DoStuff();
    }

    operation DoStuff() : Unit {
        using (q1 = Qubit()) {
            Reset(q1);
        }
    }
}

namespace 
AllocationTagging5 {
    open Microsoft.Quantum.Intrinsic;
    operation DoStuff() : Unit {
        using ((q1, qs, q2) = (Qubit(), Qubit[1], Qubit())) {
            ResetAll([q1, qs[0], q2]);
        }
    }
}

namespace AllocationTagging6 
{ open Microsoft.Quantum.Intrinsic;
    operation DoStuff() : Unit {
        using (q = Qubit()) {
            Reset(q);
        }
    }
}

namespace 
AllocationTagging7
{

    open Microsoft.Quantum.Intrinsic;

    operation DoStuff() : Unit {
        using ((x1, (x2, (x3))) = (Qubit(), (Qubit(), Qubit()))) {
            ResetAll([x3, x1, x2]);
        }
    }
}

