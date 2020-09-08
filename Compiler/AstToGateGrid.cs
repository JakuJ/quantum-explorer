using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Quantum.QsCompiler.DataTypes;
using Microsoft.Quantum.QsCompiler.SyntaxTokens;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.Quantum.QsCompiler.Transformations.Core;

namespace Compiler
{
    /// <summary>
    /// This class exports methods responsible for extracting information about quantum circuits from an AST.
    /// </summary>
    public static class AstToGateGrid
    {
        /// <summary>
        /// Get quantum gate grids for each operation defined in the namespace.
        /// </summary>
        /// <param name="compilation">The root object of an AST.</param>
        /// <returns>A dictionary mapping operation names to grids.</returns>
        public static Dictionary<string, GateGrid> GetGrids(QsCompilation compilation)
        {
            var transform = new Transform(new TransformationState());

            // TODO: Support for multiple namespaces
            transform.Namespaces.OnNamespace(compilation.Namespaces.First());

            return transform.SharedState.Functions;
        }

        /// <summary>
        /// Class used to track the internal state of the transformation, as well as access any information based on it.
        /// These properties and methods are usually used by multiple subtransformations.
        /// </summary>
        private class TransformationState
        {
            private string currentFunction = "Undefined";

            public string CurrentFunction
            {
                get => currentFunction;
                set
                {
                    currentFunction = value;
                    if (!Functions.ContainsKey(CurrentFunction))
                    {
                        Functions[CurrentFunction] = new GateGrid(1);
                    }
                }
            }

            public Dictionary<string, GateGrid> Functions { get; } = new Dictionary<string, GateGrid>();

            public void AddGate(string gate) => Functions[CurrentFunction].AddGate(0, new QuantumGate(gate));
        }

        private class Transform : SyntaxTreeTransformation<TransformationState>
        {
            public Transform(TransformationState state) : base(state)
            {
                Namespaces = new NamespaceTransformation(this);
                ExpressionKinds = new ExpressionKindTransformation(this);

                Statements = new StatementTransformation<TransformationState>(this, TransformationOptions.NoRebuild);
                StatementKinds = new StatementKindTransformation<TransformationState>(this, TransformationOptions.NoRebuild);
                Expressions = new ExpressionTransformation<TransformationState>(this, TransformationOptions.NoRebuild);
                Types = new TypeTransformation<TransformationState>(this, TransformationOptions.Disabled);
            }

            /// <summary>
            /// Class that defines expression kind transformations for ListIdentifiers.
            /// It adds any identifier that occurs within an expression to the shared transformation state.
            /// The transformation does not modify the syntax tree nodes.
            /// </summary>
            private class ExpressionKindTransformation : ExpressionKindTransformation<TransformationState>
            {
                internal ExpressionKindTransformation(Transform parent) : base(parent, TransformationOptions.NoRebuild) { }

                public override QsExpressionKind<TypedExpression, Identifier, ResolvedType> OnIdentifier(
                    Identifier sym, QsNullable<ImmutableArray<ResolvedType>> tArgs)
                {
                    string? name =
                        sym switch
                        {
                            Identifier.LocalVariable var     => var.Item.Value,
                            Identifier.GlobalCallable global => global.Item.ToString(),
                            _                                => null,
                        };

                    string[] prefixes = { "Microsoft.Quantum.Intrinsic", "Microsoft.Quantum.Measurement" };
                    if (name != null && prefixes.Any(x => name.StartsWith(x)))
                    {
                        SharedState.AddGate(name);
                    }

                    return base.OnIdentifier(sym, tArgs);
                }
            }

            private class NamespaceTransformation : NamespaceTransformation<TransformationState>
            {
                public NamespaceTransformation(Transform parent) : base(parent, TransformationOptions.NoRebuild) { }

                public override QsCallable OnCallableDeclaration(QsCallable c)
                {
                    SharedState.CurrentFunction = c.FullName.Name.Value;
                    return base.OnCallableDeclaration(c);
                }
            }
        }
    }
}
