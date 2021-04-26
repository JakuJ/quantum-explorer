namespace Ns1 {
    @EntryPoint()
    operation Op1(): Unit {
        use q = Qubit() {
            Ns2.Op1(q);
            Ns3.Op2(q);
        }
    }
}

namespace Ns2 {
    operation Op1(q: Qubit): Unit {
        Op2(q);
    }
    operation Op2(q: Qubit): Unit {}
}

namespace Ns3 {
    operation Op2(q: Qubit): Unit {}
}
