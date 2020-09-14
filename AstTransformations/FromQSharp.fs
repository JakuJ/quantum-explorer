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
                    Map.tryFind this.OpName this.Operations |> flip defaultArg null
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
            this.SharedState.QubitId <-
                match scope.Binding.Lhs with
                | VariableName name -> name.Value
                | _ -> failwith "Unknown SymbolTuple for the qubit binding"

            let qubits =
                int32 <| match scope.Binding.Rhs.Resolution with
                         | QubitRegisterAllocation { Expression = IntLiteral num } -> num
                         | SingleQubitAllocation -> 1L
                         | _ -> failwith "Invalid qubit allocation"

            this.SharedState.Grid <- GateGrid qubits
            base.OnAllocateQubits scope

    and ExpressionKindTransform(parent: Transform) =
        inherit ExpressionKindTransformation<State>(parent, TransformationOptions.NoRebuild)

        let prefixes: Set<string> = set [ "Microsoft.Quantum.Intrinsic"; "Microsoft.Quantum.Measurement" ]

        override this.OnIdentifier(sym: Identifier, tArgs) =
            match sym with
            | GlobalCallable glob when Set.contains glob.Namespace.Value prefixes ->
                if this.SharedState.Grid <> null then this.SharedState.Grid.AddGate(0, QuantumGate glob.Name.Value)
            | _ -> ()

            base.OnIdentifier(sym, tArgs)

    let GetGates (compilation: QsCompilation): Dictionary<string, GateGrid> =
        let transform = Transform()

        compilation.Namespaces
        |> Enumerable.First
        |> transform.Namespaces.OnNamespace
        |> ignore

        transform.SharedState.Operations |> Dictionary
