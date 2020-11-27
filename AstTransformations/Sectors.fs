namespace AstTransformations

/// Exports functions and types that deal with organizing qubit identifier sectors
module Sectors =
    /// A list that can only contain unique elements
    /// Note that this is only a type alias, the uniqueness is not enforced in any way
    type UniqueList<'a> = 'a list

    /// Add an element to the end of the list or do nothing if already present
    let rec addUnique: ('a -> UniqueList<'a> -> UniqueList<'a>) =
        fun v ->
            function
            | x :: xs when x = v -> x :: xs
            | x :: xs -> x :: addUnique v xs
            | [] -> [ v ]

    /// Represents consecutive rows assigned to single qubits or ones from the same register
    type Sector = string * UniqueList<string>

    /// Add a concrete identifier to the provided register's sector
    let rec addToRegister: (string -> string -> Sector list -> Sector list) =
        fun reg qName ->
            function
            | (x, xs) :: rest when x = reg -> (x, addUnique qName xs) :: rest
            | x :: xs -> x :: addToRegister reg qName xs
            | [] -> [ (reg, [ qName ]) ]

    /// Add a singleton sector containing a qubit identifier
    let rec addSingle: (string -> Sector list -> Sector list) =
        fun qName ->
            function
            | ((x, _) :: rest) when x = qName -> (x, [ x ]) :: rest
            | x :: xs -> x :: addSingle qName xs
            | [] -> [ (qName, [ qName ]) ]
