namespace Qrng {
    open Microsoft.Quantum.Convert;
    open Microsoft.Quantum.Math;
    open Microsoft.Quantum.Measurement;
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;
    
    operation SampleQuantumRandomNumberGenerator() : Result {
        using (q = Qubit())  {
            H(q);               // Put the qubit to superposition. It now has a 50% chance of being 0 or 1.
            return MResetZ(q);  // Measure the qubit value.
        }
    }

    operation SampleRandomNumberInRange(byteSize : Int) : Int {
        mutable bits = new Result[0];
        for (idxBit in 1..byteSize) {
            set bits += [SampleQuantumRandomNumberGenerator()];
        }
        return ResultArrayAsInt(bits);
    }
    
    @EntryPoint()
    operation SampleRandomNumber() : Unit {
        let max = 50;
        Message($"Sampling a random number between 0 and {max}: ");
        let byteSize = BitSizeI(max);
        mutable number = max;
        repeat{
            set number = SampleRandomNumberInRange(byteSize);
        }
        until(number < max);
        Message($"Generated number is {number}");
    }
}
