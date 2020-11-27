namespace AstTransformations

open System.Collections.Generic
open System.Linq
open Compiler
open Helpers
open Sectors
open Microsoft.Quantum.QsCompiler.SyntaxTokens
open Microsoft.Quantum.QsCompiler.SyntaxTree
open Microsoft.Quantum.QsCompiler.Transformations.Core

module FromQSharp =
    /// An identifier that resolves to a Qubit
    /// Either a Qubit itself, or a Qubit array with an index
    /// TODO: Support qubit arrays indexed by arbitrary expressions
    type QubitRef =
        | Single of string
        | Register of string * int

    /// State shared by all AST traversal classes/methods
    type State() =
        class
            /// Currently processed namespace
            member val Namespace: string = null with get, set

            /// Currently processed operation declaration
            member val Operation: string = null with get, set

            /// Maps fully qualified operation names to GateGrids
            member val Operations: Map<string, GateGrid> = Map.empty with get, set

            /// Set this to a GateGrid to save it in the output dictionary
            member this.Grid
                with set x =
                    let fullName =
                        sprintf "%s.%s" this.Namespace this.Operation

                    this.Operations <- Map.add fullName x this.Operations

            /// A set of known qubit identifiers
            member val KnownQubits: Set<string> = Set.empty with get, set

            /// A set of known qubit register identifiers
            member val KnownRegisters: Set<string> = Set.empty with get, set

            /// A list of qubit sectors, used to keep rows in order
            member val Sectors: Sector list = List.empty with get, set

            /// An ordered list that determines where to add gates in the grid.
            member val GateQueue: ((QubitRef * QuantumGate) list) list = List.empty with get, set
        end

    /// Our custom SyntaxTreeTransformation
    type Transform(options: TransformationOptions) =
        inherit SyntaxTreeTransformation<State>(State(), options)

        new() as this =
            Transform(TransformationOptions.NoRebuild)
            then
                this.Namespaces <- NamespaceTransform this
                this.StatementKinds <- StatementKindTransform this
                this.ExpressionKinds <- ExpressionKindTransform this
                this.Expressions <- ExpressionTransformation<State>(this, TransformationOptions.NoRebuild)
                this.Statements <- StatementTransformation<State>(this, TransformationOptions.NoRebuild)
                this.Types <- TypeTransformation<State>(this, TransformationOptions.NoRebuild)

    and NamespaceTransform(parent: Transform) =
        inherit NamespaceTransformation<State>(parent, TransformationOptions.NoRebuild)

        /// Parser a callable signature to extract names of arguments that hold qubits
        let rec parseArgs (declTuple: QsTuple<LocalVariableDeclaration<QsLocalSymbol>>): (string * bool) list =
            match declTuple with
            | QsTupleItem item ->
                match item.Type.Resolution with
                | Qubit -> [ (symbolName item.VariableName, false) ]
                | ArrayType arr -> if isQubit arr then [ (symbolName item.VariableName, true) ] else []
                | _ -> []
            | QsTuple items ->
                List.ofArray (items.ToArray())
                |> List.collect parseArgs

        /// Remember namespace name on traversal
        override this.OnNamespace(ns: QsNamespace) =
            this.SharedState.Namespace <- ns.Name
            base.OnNamespace ns

        /// Process callable declarations
        override this.OnCallableDeclaration(callable: QsCallable) =
            match callable.Kind with
            | Operation ->
                // reset state, each callable is processed separately
                this.SharedState.KnownQubits <- Set.empty
                this.SharedState.KnownRegisters <- Set.empty
                this.SharedState.Sectors <- List.empty
                this.SharedState.GateQueue <- List.empty
                this.SharedState.Operation <- callable.FullName.Name

                // fill shared state based on the callable signature
                let argNames = parseArgs callable.ArgumentTuple

                for name, isReg in argNames do
                    if isReg then
                        this.SharedState.KnownRegisters <- this.SharedState.KnownRegisters.Add name
                        this.SharedState.Sectors <- this.SharedState.Sectors @ [ (name, []) ] // add an empty sector
                    else
                        this.SharedState.KnownQubits <- this.SharedState.KnownQubits.Add name
                        this.SharedState.Sectors <- addSingle name this.SharedState.Sectors

                // traverse the rest of the AST
                let retValue = base.OnCallableDeclaration callable

                let refToString =
                    function
                    | Single q -> q
                    | Register (reg, ix) -> sprintf "%s[%d]" reg ix

                let refToSector =
                    function
                    | Single q -> q
                    | Register (reg, _) -> reg

                // create and save the grid to the dictionary
                let grid = GateGrid()

                // fill sectors with identifiers
                this.SharedState.GateQueue
                |> List.collect (List.map (fst >> fun x -> (refToSector x, refToString x)))
                |> List.iter (fun (reg, full) ->
                    this.SharedState.Sectors <- addToRegister reg full this.SharedState.Sectors)

                // set qubit identifiers in the grid based on sectors
                this.SharedState.Sectors
                |> List.collect snd
                |> fun x -> List.zip [ 0 .. x.Length - 1 ] x
                |> List.iter grid.SetName

                // add gates to the grid
                for column in this.SharedState.GateQueue do
                    let col = grid.Width

                    for (i, g) in List.rev column do
                        let y = grid.IndexOfName(refToString i)
                        grid.AddGate(col, y, g)

                this.SharedState.Grid <- grid

                // return the value obtained from the traversal
                retValue
            | _ -> base.OnCallableDeclaration callable

    and StatementKindTransform(parent: Transform) =
        inherit StatementKindTransformation<State>(parent, TransformationOptions.NoRebuild)

        /// Flatten arbitrarily nested tuples of variable names
        member this.FlattenNames: (SymbolTuple -> string list) =
            function
            | VariableName name -> [ name ]
            | VariableNameTuple t ->
                List.collect this.FlattenNames
                <| List.ofArray (t.ToArray())
            | _ -> []

        /// Flatten arbitrarily nested tuples of qubit initializers
        /// Returns None for single qubit allocations, and Some x for x-qubit register allocations
        member this.FlattenQubits: (ResolvedInitializer -> (int option) list) =
            fun x ->
                match x.Resolution with
                | SingleQubitAllocation -> [ None ]
                | QubitRegisterAllocation { Expression = IntLiteral num } -> [ Some(int32 num) ]
                | QubitTupleAllocation t ->
                    List.collect this.FlattenQubits
                    <| List.ofArray (t.ToArray())
                | _ -> [] // invalid or bad register without an integer in the brackets
        // TODO: Support qubit register allocations with size given by arbitrary expressions

        /// Process qubit allocations ("using" statements)
        override this.OnAllocateQubits(scope: QsQubitScope) =
            let qubitIDs = this.FlattenNames scope.Binding.Lhs
            let qubits = this.FlattenQubits scope.Binding.Rhs

            // Update shared state based on the collected identifiers
            for qubitID, howMany in List.zip qubitIDs qubits do
                match howMany with
                | None ->
                    this.SharedState.KnownQubits <- this.SharedState.KnownQubits.Add qubitID
                    this.SharedState.Sectors <- addSingle qubitID this.SharedState.Sectors
                | Some n ->
                    this.SharedState.KnownRegisters <- this.SharedState.KnownRegisters.Add qubitID

                    this.SharedState.Sectors <-
                        List.fold (fun acc x -> addToRegister qubitID x acc) this.SharedState.Sectors
                        <| List.map (sprintf "%s[%d]" qubitID) [ 0 .. n - 1 ]

            base.OnAllocateQubits scope

    and ExpressionKindTransform(parent: Transform) =
        inherit ExpressionKindTransformation<State>(parent, TransformationOptions.NoRebuild)

        /// Parse primitive types that refer to qubits (Identifier, ArrayItem etc.)
        member this.PrimToRefs(rhs: TypedExpression): QubitRef option =
            if not (isQubit rhs.ResolvedType) then
                None
            else
                match rhs.Expression with
                | Identifier (var, _) ->
                    match var with
                    | LocalVariable local ->
                        if this.SharedState.KnownQubits.Contains local
                        then Some(Single local)
                        else failwithf "Unknown qubit identifier: %s" local
                    | _ -> None
                | ArrayItem (indexable, index) ->
                    match indexable.Expression with
                    | Identifier (var, _) ->
                        match var with
                        | LocalVariable identifier ->
                            if this.SharedState.KnownRegisters.Contains identifier then
                                match index.Expression with
                                | IntLiteral lit -> Some(Register(identifier, int32 lit))
                                | _ -> None
                            else
                                failwithf "Unknown register identifier: %s" identifier
                        | _ -> None
                    | _ -> None
                | _ -> None

        /// Parse arbitraty structures containing references to qubits
        /// This includes arrays, tuples, and indexed registers
        member this.ArgsToNames(rhs: TypedExpression): (QubitRef list * bool) list =
            match rhs.Expression with
            | Identifier _
            | ArrayItem _ ->
                match this.PrimToRefs rhs with
                | Some v -> [ [ v ], false ]
                | None -> []
            | ValueArray args ->
                if isQubit <| arrayType rhs.ResolvedType then
                    List.ofArray (args.ToArray())
                    |> List.choose this.PrimToRefs
                    |> fun x -> [ x, true ]
                else
                    []
            | ValueTuple args ->
                List.ofArray (args.ToArray())
                |> List.collect this.ArgsToNames
            | UnitValue -> [] // no arguments
            | _ -> [] // ignore other arguments

        /// Process operation calls
        override this.OnOperationCall(lhs: TypedExpression, rhs: TypedExpression) =
            // Validate whether the operation name represents a callable
            let (gate: (string * string) option) =
                match lhs.Expression with
                | Identifier (var, _) ->
                    match var with
                    | GlobalCallable glob -> Some(glob.Namespace, glob.Name)
                    | _ -> None
                | _ -> None

            // Save the operation call as a gate to be added to the grid
            if gate.IsSome then
                let qubitIDs = this.ArgsToNames rhs

                if not (List.isEmpty qubitIDs) then
                    let (ns, name) = gate.Value

                    let columnsToAdd =
                        qubitIDs
                        |> List.zip [ 0 .. qubitIDs.Length - 1 ]
                        |> List.collect (fun (i, (refs, isArr)) ->
                            List.map (fun ref -> (ref, QuantumGate(name, ns, i, isArr, lhs))) refs)

                    this.SharedState.GateQueue <- this.SharedState.GateQueue @ [ columnsToAdd ]

            base.OnOperationCall(lhs, rhs)

    /// Process a QsCompilation and return operation names and corresponding GateGrids
    let GetGates (compilation: QsCompilation): Dictionary<string, GateGrid> =
        let transform = Transform()

        let namespaces =
            List.filter (fun ns -> not (ns.Name.StartsWith("Microsoft.Quantum")))
                (List.ofArray (compilation.Namespaces.ToArray()))

        List.iter (transform.Namespaces.OnNamespace >> ignore) namespaces

        transform.SharedState.Operations |> Dictionary
