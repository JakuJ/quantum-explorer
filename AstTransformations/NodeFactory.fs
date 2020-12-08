namespace AstTransformations

open System.Collections.Immutable
open Microsoft.Quantum.QsCompiler.SyntaxTree
open Microsoft.Quantum.QsCompiler.DataTypes
open Microsoft.Quantum.QsCompiler.SyntaxTokens

module NodeFactory =
    let MakeTagCall: (string * bool -> QsStatement) =
        fun (qubitID, isRegister) ->
            let name: QsQualifiedName =
                { Namespace = "Simulator.Custom"
                  Name = "TagAllocation" }

            let unitType = ResolvedType.New(UnitType)

            let stringArg: TypedExpression =
                { Expression = StringLiteral(qubitID, ImmutableArray.Empty)
                  TypeArguments =
                      ImmutableArray<QsQualifiedName * string * ResolvedType>
                          .Empty
                  ResolvedType = ResolvedType.New(QsTypeKind.String)
                  InferredInformation =
                      { IsMutable = false
                        HasLocalQuantumDependency = false }
                  Range = QsNullable<Range>.Null }

            let boolArg: TypedExpression =
                { Expression = BoolLiteral isRegister
                  TypeArguments =
                      ImmutableArray<QsQualifiedName * string * ResolvedType>
                          .Empty
                  ResolvedType = ResolvedType.New(QsTypeKind.Bool)
                  InferredInformation =
                      { IsMutable = false
                        HasLocalQuantumDependency = false }
                  Range = QsNullable<Range>.Null }

            let argArray: ImmutableArray<TypedExpression> =
                ImmutableArray.Create(stringArg, boolArg)

            let argTypes: ImmutableArray<ResolvedType> =
                ImmutableArray.Create(ResolvedType.New(String), ResolvedType.New(Bool))

            let args: TypedExpression =
                { Expression = ValueTuple argArray
                  TypeArguments =
                      ImmutableArray<QsQualifiedName * string * ResolvedType>
                          .Empty
                  ResolvedType = ResolvedType.New(QsTypeKind.TupleType(argTypes))
                  InferredInformation =
                      { IsMutable = false
                        HasLocalQuantumDependency = false }
                  Range = QsNullable<Range>.Null }

            let inferredInfo: InferredCallableInformation =
                { IsSelfAdjoint = false
                  IsIntrinsic = true }

            let callableInfo: CallableInformation =
                { InferredInformation = inferredInfo
                  Characteristics = ResolvedCharacteristics.Empty }

            let identifier =
                { Expression =
                      Identifier(Identifier.GlobalCallable(name), QsNullable<ImmutableArray<ResolvedType>>.Null)
                  TypeArguments =
                      ImmutableArray<QsQualifiedName * string * ResolvedType>
                          .Empty
                  ResolvedType = ResolvedType.New(QsTypeKind.Operation((args.ResolvedType, unitType), callableInfo))
                  InferredInformation =
                      { IsMutable = false
                        HasLocalQuantumDependency = false }
                  Range = QsNullable<Range>.Null }

            let callExpression: TypedExpression =
                { Expression = CallLikeExpression(identifier, args)
                  TypeArguments =
                      ImmutableArray<QsQualifiedName * string * ResolvedType>
                          .Empty
                  ResolvedType = unitType
                  InferredInformation =
                      { IsMutable = false
                        HasLocalQuantumDependency = true }
                  Range = QsNullable<Range>.Null }

            // Define custom statements
            let statement: QsStatement =
                { Statement = callExpression |> QsExpressionStatement
                  SymbolDeclarations = LocalDeclarations.Empty
                  Location = QsNullable.Null
                  Comments = QsComments.Empty }

            statement
