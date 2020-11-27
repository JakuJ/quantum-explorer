namespace AstTransformations

open Microsoft.Quantum.QsCompiler.SyntaxTokens
open Microsoft.Quantum.QsCompiler.SyntaxTree

/// Helper functions used across the project
module Helpers =
    /// Get the array element type if provided ResolvedType is ArrayType
    let arrayType (resType: ResolvedType) =
        match resType.Resolution with
        | ArrayType t -> t
        | _ -> failwith "Argument type is not ArrayType"

    /// Check if the provided ResolvedType is Qubit
    let isQubit (resType: ResolvedType): bool =
        match resType.Resolution with
        | Qubit -> true
        | _ -> false

    /// Return the name contained within a QsLocalSymbol
    let symbolName (symbol: QsLocalSymbol): string =
        match symbol with
        | ValidName name -> name
        | InvalidName -> failwith "Invalid symbol name"
