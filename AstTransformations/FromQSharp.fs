namespace AstTransformations

open System.Collections.Generic
open System.Linq
open Compiler
open Helpers
open Microsoft.Quantum.QsCompiler.SyntaxTokens
open Microsoft.Quantum.QsCompiler.SyntaxTree
open Microsoft.Quantum.QsCompiler.Transformations.Core

module FromQSharp =
    type State() =
        class
            member val Namespace: string = null with get, set
            member val Operation: string = null with get, set

            member val Operations: Map<string, GateGrid> = Map.empty with get, set

            member val KnownQubits: Set<string> = Set.empty with get, set
            member val KnownRegisters: Set<string> = Set.empty with get, set

            member this.Grid
                with get (): GateGrid =
                    Map.tryFind this.Operation this.Operations
                    |> flip defaultArg null
                and set (value: GateGrid) =
                    this.Operations <- Map.add this.Operation value this.Operations
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
                this.SharedState.KnownQubits <- Set.empty
                this.SharedState.KnownRegisters <- Set.empty

                this.SharedState.Operation <- sprintf "%s.%s" this.SharedState.Namespace callable.FullName.Name.Value
                this.SharedState.Grid <- GateGrid()

                let argNames = parseArgs callable.ArgumentTuple

                for (name, isReg), ix in List.zip argNames [ 0 .. argNames.Length - 1 ] do
                    if isReg then
                        this.SharedState.KnownRegisters <- this.SharedState.KnownRegisters.Add name
                    else
                        this.SharedState.Grid.SetName(ix, name)
                        this.SharedState.KnownQubits <- this.SharedState.KnownQubits.Add name
            | _ -> ()

            base.OnCallableDeclaration callable

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

            let grid = GateGrid()

            if qubits = 1 then
                grid.SetName(0, qubitID)
                this.SharedState.KnownQubits <- this.SharedState.KnownQubits.Add qubitID
            else
                List.map (fun x -> (x, sprintf "%s[%d]" qubitID x)) [ 0 .. qubits - 1 ]
                |> List.iter grid.SetName
                this.SharedState.KnownRegisters <- this.SharedState.KnownRegisters.Add qubitID

            this.SharedState.Grid <- grid
            base.OnAllocateQubits scope

    and ExpressionKindTransform(parent: Transform) =
        inherit ExpressionKindTransformation<State>(parent, TransformationOptions.NoRebuild)

        member this.ArgsToNames(rhs: TypedExpression): string list =
            match rhs.Expression with
            | Identifier (var, _) ->
                match var with
                | LocalVariable local ->
                    if this.SharedState.KnownQubits.Contains local.Value
                    then [ local.Value ]
                    else failwith "Unknown qubit identifier"
                | _ -> failwith "Only local variable identifiers supported arguments to operation calls"
            | ArrayItem (indexable, index) ->
                match indexable.Expression with
                | Identifier (var, _) ->
                    match var with
                    | LocalVariable identifier ->
                        if this.SharedState.KnownRegisters.Contains identifier.Value then
                            match index.Expression with
                            | IntLiteral lit -> [ sprintf "%s[%d]" identifier.Value lit ]
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

            if gate.IsSome && not (isNull this.SharedState.Grid) then
                let qubitIDs = this.ArgsToNames rhs
                if not (List.isEmpty qubitIDs) then
                    let (ns, name) = gate.Value

                    let mutable ix =
                        this.SharedState.Grid.IndexOfName
                        <| List.head qubitIDs

                    if ix = -1 then // This register has not yet been added to the grid
                        ix <- this.SharedState.Grid.Height
                        for ident in qubitIDs do
                            this.SharedState.Grid.SetName(this.SharedState.Grid.Height, ident)

                    this.SharedState.Grid.AddGate(ix, QuantumGate(name, ns, List.length qubitIDs, lhs))

            base.OnOperationCall(lhs, rhs)

    let GetGates (compilation: QsCompilation): Dictionary<string, GateGrid> =
        let transform = Transform()

        let namespaces =
            List.filter (fun ns -> not (ns.Name.Value.StartsWith("Microsoft.Quantum")))
                (List.ofArray (compilation.Namespaces.ToArray()))

        List.iter (transform.Namespaces.OnNamespace >> ignore) namespaces

        transform.SharedState.Operations |> Dictionary
