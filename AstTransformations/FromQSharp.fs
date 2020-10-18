namespace AstTransformations

open System.Collections.Generic
open System.Linq
open Compiler
open Helpers
open Microsoft.Quantum.QsCompiler.SyntaxTokens
open Microsoft.Quantum.QsCompiler.SyntaxTree
open Microsoft.Quantum.QsCompiler.Transformations.Core

module FromQSharp =
    type Sectors = (string * string list) list

    let rec addUnique (v: 'a) =
        function
        | x :: xs when x = v -> x :: xs
        | x :: xs -> x :: addUnique v xs
        | [] -> [ v ]

    let rec addToSector (sectors: Sectors) (secName: string) (qName: string): Sectors =
        match sectors with
        | (x, xs) :: rest when x = secName -> (x, addUnique qName xs) :: rest
        | x :: xs -> x :: addToSector xs secName qName
        | [] -> [ (secName, [ qName ]) ]

    type QubitRef =
        | Single of string
        | Register of string * int

    type State() =
        class
            member val Namespace: string = null with get, set
            member val Operation: string = null with get, set

            member val Operations: Map<string, GateGrid> = Map.empty with get, set

            member val KnownQubits: Set<string> = Set.empty with get, set
            member val KnownRegisters: Set<string> = Set.empty with get, set

            member val Sectors: Sectors = List.empty with get, set
            member val GateQueue: (QubitRef * QuantumGate) list = List.empty with get, set
        end

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

        override this.OnNamespace(ns: QsNamespace) =
            this.SharedState.Namespace <- ns.Name.Value
            base.OnNamespace ns

        override this.OnCallableDeclaration(callable: QsCallable) =
            match callable.Kind with
            | Operation ->
                // reset state
                this.SharedState.KnownQubits <- Set.empty
                this.SharedState.KnownRegisters <- Set.empty
                this.SharedState.Sectors <- List.empty
                this.SharedState.GateQueue <- List.empty
                this.SharedState.Operation <- sprintf "%s.%s" this.SharedState.Namespace callable.FullName.Name.Value

                // parse qubit and register arguments
                let argNames = parseArgs callable.ArgumentTuple

                for name, isReg in argNames do
                    if isReg then
                        this.SharedState.KnownRegisters <- this.SharedState.KnownRegisters.Add name
                        this.SharedState.Sectors <- this.SharedState.Sectors @ [ (name, []) ] // add an empty sector
                    else
                        this.SharedState.KnownQubits <- this.SharedState.KnownQubits.Add name
                        this.SharedState.Sectors <- addToSector this.SharedState.Sectors name name

                // traverse the rest of the AST
                let retValue = base.OnCallableDeclaration callable

                // create and save the grid to the dictionary
                let grid = GateGrid()

                let refToString =
                    function
                    | Single q -> q
                    | Register (reg, ix) -> sprintf "%s[%d]" reg ix

                let refToSector =
                    function
                    | Single q -> q
                    | Register (reg, _) -> reg

                this.SharedState.GateQueue
                |> List.map (fst >> (fun x -> (refToSector x, refToString x)))
                |> List.iter (fun (reg, full) ->
                    this.SharedState.Sectors <- addToSector this.SharedState.Sectors reg full)

                this.SharedState.Sectors
                |> List.collect snd
                |> fun x -> List.zip [ 0 .. x.Length - 1 ] x
                |> List.iter grid.SetName

                this.SharedState.GateQueue
                |> List.map (fun (i, g) -> (grid.IndexOfName(refToString i), g))
                |> List.iter grid.AddGate

                this.SharedState.Operations <- Map.add this.SharedState.Operation grid this.SharedState.Operations

                // return the value obtained from the traversal
                retValue
            | _ -> base.OnCallableDeclaration callable



    and StatementKindTransform(parent: Transform) =
        inherit StatementKindTransformation<State>(parent, TransformationOptions.NoRebuild)

        override this.OnAllocateQubits(scope: QsQubitScope) =
            let qubitID =
                match scope.Binding.Lhs with
                | VariableName name -> name.Value
                | _ -> failwith "Unknown SymbolTuple for the qubit binding"

            let qubits =
                int32
                <| match scope.Binding.Rhs.Resolution with
                   | QubitRegisterAllocation { Expression = IntLiteral num } -> num
                   | SingleQubitAllocation -> 1L
                   | _ -> failwith "Invalid qubit allocation"

            if qubits = 1 then
                this.SharedState.KnownQubits <- this.SharedState.KnownQubits.Add qubitID
                this.SharedState.Sectors <- addToSector this.SharedState.Sectors qubitID qubitID
            else
                this.SharedState.KnownRegisters <- this.SharedState.KnownRegisters.Add qubitID
                this.SharedState.Sectors <-
                    List.fold (fun acc x -> addToSector acc qubitID x) this.SharedState.Sectors
                    <| List.map (sprintf "%s[%d]" qubitID) [ 0 .. qubits - 1 ]

            base.OnAllocateQubits scope

    and ExpressionKindTransform(parent: Transform) =
        inherit ExpressionKindTransformation<State>(parent, TransformationOptions.NoRebuild)

        member this.ArgsToNames(rhs: TypedExpression): QubitRef list =
            match rhs.Expression with
            | Identifier (var, _) ->
                match var with
                | LocalVariable local ->
                    if this.SharedState.KnownQubits.Contains local.Value
                    then [ Single local.Value ]
                    else failwith "Unknown qubit identifier"
                | _ -> failwith "Only local variable identifiers supported arguments to operation calls"
            | ArrayItem (indexable, index) ->
                match indexable.Expression with
                | Identifier (var, _) ->
                    match var with
                    | LocalVariable identifier ->
                        if this.SharedState.KnownRegisters.Contains identifier.Value then
                            match index.Expression with
                            | IntLiteral lit -> [ Register(identifier.Value, int32 lit) ]
                            | _ -> failwith "Array index is not an integer"
                        else
                            failwith "Unknown register identifier"
                    | _ -> failwith "Global operation arguments not supported"
                | _ -> failwith "Only local variable identifiers supported as arguments to operation calls"
            | ValueArray args ->
                if isQubit <| arrayType rhs.ResolvedType then
                    List.ofArray (args.ToArray())
                    |> List.collect this.ArgsToNames
                else
                    []
            | ValueTuple args -> // multiple arguments
                List.ofArray (args.ToArray())
                |> List.collect this.ArgsToNames
            | UnitValue -> [] // no arguments
            | x ->
                failwithf "Unexpected argument to an operation call: %s"
                <| string x

        override this.OnOperationCall(lhs: TypedExpression, rhs: TypedExpression) =
            let (gate: (string * string) option) =
                match lhs.Expression with
                | Identifier (var, _) ->
                    match var with
                    | GlobalCallable glob -> Some(glob.Namespace.Value, glob.Name.Value)
                    | _ -> None
                | _ -> None

            if gate.IsSome then
                let qubitIDs = this.ArgsToNames rhs
                if not (List.isEmpty qubitIDs) then
                    let (ns, name) = gate.Value

                    let g =
                        QuantumGate(name, ns, List.length qubitIDs, lhs)


                    this.SharedState.GateQueue <-
                        this.SharedState.GateQueue
                        @ [ (qubitIDs.Head, g) ]

            base.OnOperationCall(lhs, rhs)

    let GetGates (compilation: QsCompilation): Dictionary<string, GateGrid> =
        let transform = Transform()

        let namespaces =
            List.filter (fun ns -> not (ns.Name.Value.StartsWith("Microsoft.Quantum")))
                (List.ofArray (compilation.Namespaces.ToArray()))

        List.iter (transform.Namespaces.OnNamespace >> ignore) namespaces

        transform.SharedState.Operations |> Dictionary
