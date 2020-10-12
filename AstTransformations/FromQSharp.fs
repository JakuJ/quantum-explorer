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
            let mutable operation: string = null

            member this.OpName
                with get () = operation
                and set (value: string) =
                    operation <- value
                    this.Grid <- null // just add the key to the dictionary

            member val Operations: Map<string, GateGrid> = Map.empty with get, set
            member val QubitId: string = null with get, set

            member this.Grid
                with get (): GateGrid =
                    Map.tryFind this.OpName this.Operations
                    |> flip defaultArg null
                and set (value: GateGrid) =
                    this.Operations <- Map.add this.OpName value this.Operations
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

        override this.OnCallableDeclaration(callable: QsCallable) =
            match callable.Kind with
            | Operation -> this.SharedState.OpName <- callable.FullName.Name.Value
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
            else
                List.map (fun x -> (x, sprintf "%s[%d]" qubitID x)) [ 0 .. qubits - 1 ]
                |> List.iter grid.SetName

            this.SharedState.Grid <- grid
            this.SharedState.QubitId <- qubitID

            base.OnAllocateQubits scope

    and ExpressionKindTransform(parent: Transform) =
        inherit ExpressionKindTransformation<State>(parent, TransformationOptions.NoRebuild)

        let prefixes: Set<string> =
            set [ "Microsoft.Quantum.Intrinsic"
                  "Microsoft.Quantum.Measurement" ]

        override this.OnOperationCall(lhs: TypedExpression, rhs: TypedExpression) =
            let (gate: (string * string) option) =
                match lhs.Expression with
                | Identifier (var, _) ->
                    match var with
                    | GlobalCallable glob when Set.contains glob.Namespace.Value prefixes ->
                        Some(glob.Namespace.Value, glob.Name.Value)
                    | _ -> None
                | _ -> None

            if gate.IsSome && not (isNull this.SharedState.Grid) then
                let qubitID =
                    match rhs.Expression with
                    | Identifier (var, _) ->
                        match var with
                        | LocalVariable local -> local.Value
                        | _ -> failwith "Only local variable identifiers supported arguments to operation calls"
                    | ArrayItem (indexable, index) ->
                        match indexable.Expression with
                        | Identifier (var, _) ->
                            match var with
                            | LocalVariable identifier ->
                                match index.Expression with
                                | IntLiteral lit -> sprintf "%s[%d]" identifier.Value lit
                                | _ -> failwith "Array index is not an integer"
                            | _ -> failwith "Global operation arguments not supported"
                        | _ -> failwith "Only local variable identifiers supported arguments to operation calls"
                    | _ -> failwith "Invalid argument to a operation call"

                let (ns, name) = gate.Value;
                let ix = this.SharedState.Grid.IndexOfName qubitID
                this.SharedState.Grid.AddGate(ix, QuantumGate (name, ns, 1, lhs))

            base.OnOperationCall(lhs, rhs)

    let GetGates (compilation: QsCompilation): Dictionary<string, GateGrid> =
        let transform = Transform()

        let ns = Enumerable.First compilation.Namespaces

        if ns.Name.Value.StartsWith("Microsoft.Quantum") then
            Map.empty // return an empty dictionary if there's no valid namespace
        else
            ns |> transform.Namespaces.OnNamespace |> ignore
            transform.SharedState.Operations
        |> Dictionary
