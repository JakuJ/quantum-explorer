namespace AstTransformations

open System.Collections.Immutable
open System.Linq
open Microsoft.Quantum.QsCompiler.SyntaxTokens
open Microsoft.Quantum.QsCompiler.SyntaxTree
open Microsoft.Quantum.QsCompiler.Transformations.Core
open TagCallCreation

module TagAllocations =
    /// State shared by all AST traversal classes/methods
    type State() =
        class
            /// Currently processed operation declaration
            member val Operation: string = null with get, set
        end

    /// Our custom SyntaxTreeTransformation
    type Transform(options: TransformationOptions) =
        inherit SyntaxTreeTransformation<State>(State(), options)

        new() as this =
            Transform(TransformationOptions.Default)
            then
                this.Namespaces <- NamespaceTransformation<State>(this, TransformationOptions.Default)
                this.StatementKinds <- StatementKindTransform this
                this.ExpressionKinds <- ExpressionKindTransformation<State>(this, TransformationOptions.Default)
                this.Expressions <- ExpressionTransformation<State>(this, TransformationOptions.Default)
                this.Statements <- StatementTransformation<State>(this, TransformationOptions.Default)
                this.Types <- TypeTransformation<State>(this, TransformationOptions.Default)

    and StatementKindTransform(parent: Transform) =
        inherit StatementKindTransformation<State>(parent, TransformationOptions.Default)

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
        member this.FlattenQubits: (ResolvedInitializer -> bool list) =
            fun x ->
                match x.Resolution with
                | SingleQubitAllocation -> [ false ]
                | QubitRegisterAllocation _ -> [ true ]
                | QubitTupleAllocation t ->
                    List.collect this.FlattenQubits
                    <| List.ofArray (t.ToArray())
                | x ->
                    failwithf "Unexpected qubit allocation: %s"
                    <| string x

        /// Process qubit allocations ("using" statements)
        override this.OnAllocateQubits(scope: QsQubitScope) =
            let qubitIDs = this.FlattenNames scope.Binding.Lhs
            let qubits = this.FlattenQubits scope.Binding.Rhs

            let statements =
                List.zip qubitIDs qubits |> List.map CreateTagCall

            // Add custom operation calls to the body of the scope
            let newStatements =
                statements
                @ List.ofArray (scope.Body.Statements.ToArray())

            let newScope =
                { scope with
                      Body =
                          { scope.Body with
                                Statements = newStatements.ToImmutableArray() } }

            base.OnAllocateQubits newScope

    /// Process a QsCompilation and return operation names and corresponding GateGrids
    let TagAllocationsInCompilation (compilation: QsCompilation): QsCompilation =
        let transform = Transform()

        let namespaces =
            compilation.Namespaces.ToArray()
            |> List.ofArray
            |> List.map (fun ns ->
                if ((not (ns.Name.StartsWith("Microsoft.Quantum")))
                    && ns.Name <> "Simulator.Utils") then
                    transform.Namespaces.OnNamespace ns
                else
                    ns)

        { compilation with
              Namespaces = namespaces.ToImmutableArray() }
