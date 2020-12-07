namespace AstTransformations

open System.Collections.Immutable
open System.Linq
open Microsoft.Quantum.QsCompiler.DataTypes
open Microsoft.Quantum.QsCompiler.SyntaxTokens
open Microsoft.Quantum.QsCompiler.SyntaxTree
open Microsoft.Quantum.QsCompiler.Transformations.Core

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
            Transform(TransformationOptions.NoRebuild)
            then
                this.Namespaces <- NamespaceTransformation<State>(this, TransformationOptions.NoRebuild)
                this.StatementKinds <- StatementKindTransform this
                this.ExpressionKinds <- ExpressionKindTransformation<State>(this, TransformationOptions.NoRebuild)
                this.Expressions <- ExpressionTransformation<State>(this, TransformationOptions.NoRebuild)
                this.Statements <- StatementTransformation<State>(this, TransformationOptions.NoRebuild)
                this.Types <- TypeTransformation<State>(this, TransformationOptions.NoRebuild)

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
                | None -> failwith "single qubit"
                | Some n -> failwith "multiple qubits" // List.map (sprintf "%s[%d]" qubitID) [ 0 .. n - 1 ]

            // Define custom statements
            let callExpression: TypedExpression = failwith "a"

            let callMessage: QsStatement =
                { Statement = callExpression |> QsExpressionStatement
                  SymbolDeclarations = LocalDeclarations.Empty
                  Location = QsNullable.Null
                  Comments = QsComments.Empty }

            let statements = [ callMessage ]

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
