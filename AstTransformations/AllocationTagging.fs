namespace AstTransformations

open System.Collections.Immutable
open System.Linq
open Microsoft.Quantum.QsCompiler.SyntaxTokens
open Microsoft.Quantum.QsCompiler.SyntaxTree
open Microsoft.Quantum.QsCompiler.Transformations.Core
open NodeCreation

module AllocationTagging =
    /// State shared by all AST traversal classes/methods
    type State() =
        class
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

        /// Flatten arbitrarily nested tuples of variable names to a list of strings
        member this.FlattenNames: (SymbolTuple -> string list) =
            function
            | VariableName name -> [ name ]
            | VariableNameTuple t ->
                List.collect this.FlattenNames
                <| List.ofArray (t.ToArray())
            | _ -> []

        /// Flatten arbitrarily nested tuples of qubit initializers
        /// Returns false for single qubit allocations, and true for register allocations
        member this.FlattenQubits: (ResolvedInitializer -> bool list) =
            fun x ->
                match x.Resolution with
                | SingleQubitAllocation -> [ false ]
                | QubitRegisterAllocation _ -> [ true ]
                | QubitTupleAllocation t ->
                    List.collect this.FlattenQubits
                    <| List.ofArray (t.ToArray())
                | InvalidInitializer -> failwithf "Invalid qubit initializer"

        /// Process qubit allocations ("using" statements)
        override this.OnAllocateQubits(scope: QsQubitScope) =
            let qubitIDs = this.FlattenNames scope.Binding.Lhs
            let registerFlags = this.FlattenQubits scope.Binding.Rhs

            let tagCallStatements =
                registerFlags
                |> List.zip qubitIDs
                |> List.map MakeTagCall

            // Add custom operation calls to the body of the scope
            let newStatements =
                tagCallStatements
                @ List.ofArray (scope.Body.Statements.ToArray())

            let newScope =
                { scope with
                      Body =
                          { scope.Body with
                                Statements = newStatements.ToImmutableArray() } }

            base.OnAllocateQubits newScope

    /// Process a QsCompilation and return operation names and corresponding GateGrids
    let TagAllocations (compilation: QsCompilation): QsCompilation =
        let transform = Transform()

        let notSkipped ns =
            not (ns.Name.StartsWith("Microsoft.Quantum"))
            && ns.Name <> "Simulator.Utils"

        let namespaces =
            compilation.Namespaces.ToArray()
            |> List.ofArray
            |> List.map (fun ns -> if notSkipped ns then transform.Namespaces.OnNamespace ns else ns)

        { compilation with
              Namespaces = namespaces.ToImmutableArray() }
