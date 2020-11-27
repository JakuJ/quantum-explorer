namespace AstTransformations

open Microsoft.Quantum.QsCompiler.SyntaxTokens
open Microsoft.Quantum.QsCompiler.SyntaxTree

module Helpers =
    let arrayType (resType: ResolvedType) =
        match resType.Resolution with
        | ArrayType t -> t
        | _ -> failwith "Argument type is not ArrayType"

    let isQubit (resType: ResolvedType): bool =
        match resType.Resolution with
        | Qubit -> true
        | _ -> false

    let symbolName (symbol: QsLocalSymbol): string =
        match symbol with
        | ValidName name -> name
        | InvalidName -> failwith "Invalid symbol name"
