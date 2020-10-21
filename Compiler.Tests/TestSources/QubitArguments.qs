namespace QubitArguments {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;

    operation Op1(target : Qubit) : Unit {
        H(target);
    }

    operation Op2(target1 : Qubit, target2: Qubit) : Unit {
        H(target2);
        H(target1);
    }

    operation OpReg1(targets : Qubit[]) : Unit {
        H(targets[0]);
    }

    operation OpReg2(targets : Qubit[]) : Unit {
        H(targets[0]);
        H(targets[1]);
    }

    operation OpReg5(targets : Qubit[]) : Unit {
        H(targets[2]);
        H(targets[4]);
        H(targets[1]);
        H(targets[3]);
        H(targets[0]);
    }

}
