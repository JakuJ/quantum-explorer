﻿namespace Library {

    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;
    open Microsoft.Quantum.Convert;
    open Microsoft.Quantum.Math;

    @EntryPoint()
    operation Main() : Unit {
        let x = RandomInt(12);
        DoNothing();
    }
    
    function DoNothing() : Unit {}
    
    operation RandomBit () : Result {
        use q = Qubit() {
            H(q);
            return MResetZ(q);
        }
    }

    operation RandomInt(max: Int) : Int {
        mutable result = 0;
        repeat {
            mutable bits = new Result[0];
            for i in 1 .. BitSizeI(max) {
                set bits += [RandomBit()];
            }
            set result = ResultArrayAsInt(bits);
        } until (result <= max);
        return result;
    }
}
