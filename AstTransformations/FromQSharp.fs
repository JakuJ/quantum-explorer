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
    /// Am identifier that resolves to a Qubit
    /// Either a Qubit itself, or Qubit[] with index
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
                this.SharedState.Operation <- callable.FullName.Name.Value

                // parse qubit and register arguments
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

                this.SharedState.GateQueue
                |> List.collect
                    (List.map
                        (fst
                         >> fun x -> (refToSector x, refToString x)))
                |> List.iter (fun (reg, full) ->
                    this.SharedState.Sectors <- addToRegister reg full this.SharedState.Sectors)

                this.SharedState.Sectors
                |> List.collect snd
                |> fun x -> List.zip [ 0 .. x.Length - 1 ] x
                |> List.iter grid.SetName

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
                this.SharedState.Sectors <- addSingle qubitID this.SharedState.Sectors
            else
                this.SharedState.KnownRegisters <- this.SharedState.KnownRegisters.Add qubitID
                this.SharedState.Sectors <-
                    List.fold (fun acc x -> addToRegister qubitID x acc) this.SharedState.Sectors
                    <| List.map (sprintf "%s[%d]" qubitID) [ 0 .. qubits - 1 ]

            base.OnAllocateQubits scope

    and ExpressionKindTransform(parent: Transform) =
        inherit ExpressionKindTransformation<State>(parent, TransformationOptions.NoRebuild)

        member this.PrimToRefs(rhs: TypedExpression): QubitRef option =
            if not (isQubit rhs.ResolvedType) then
                None
            else
                match rhs.Expression with
                | Identifier (var, _) ->
                    match var with
                    | LocalVariable local ->
                        if this.SharedState.KnownQubits.Contains local.Value
                        then Some(Single local.Value)
                        else failwithf "Unknown qubit identifier: %s" local.Value
                    | _ -> None
                | ArrayItem (indexable, index) ->
                    match indexable.Expression with
                    | Identifier (var, _) ->
                        match var with
                        | LocalVariable identifier ->
                            if this.SharedState.KnownRegisters.Contains identifier.Value then
                                match index.Expression with
                                | IntLiteral lit -> Some(Register(identifier.Value, int32 lit))
                                | _ -> None
                            else
                                failwithf "Unknown register identifier: %s" identifier.Value
                        | _ -> None
                    | _ -> None
                | _ -> None

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
            | ValueTuple args -> // multiple arguments
                List.ofArray (args.ToArray())
                |> List.collect this.ArgsToNames
            | UnitValue -> [] // no arguments
            | _ -> [] // ignore other arguments

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

                    let columnsToAdd =
                        qubitIDs
                        |> List.zip [ 0 .. qubitIDs.Length - 1 ]
                        |> List.collect (fun (i, (refs, isArr)) ->
                            List.map (fun ref -> (ref, QuantumGate(name, ns, i, isArr, lhs))) refs)

                    this.SharedState.GateQueue <- this.SharedState.GateQueue @ [ columnsToAdd ]

            base.OnOperationCall(lhs, rhs)

    let GetGates (compilation: QsCompilation): Dictionary<string, GateGrid> =
        let transform = Transform()

        let namespaces =
            List.filter (fun ns -> not (ns.Name.Value.StartsWith("Microsoft.Quantum")))
                (List.ofArray (compilation.Namespaces.ToArray()))

        List.iter (transform.Namespaces.OnNamespace >> ignore) namespaces

        transform.SharedState.Operations |> Dictionary
