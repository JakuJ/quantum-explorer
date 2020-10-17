namespace AstTransformations

open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Reflection
open Microsoft.Quantum.QsCompiler.SyntaxTokens
open Microsoft.Quantum.QsCompiler.SyntaxTree

module Helpers =
    let flip = fun f a b -> f b a

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
        | ValidName name -> name.Value
        | InvalidName -> failwith "Invalid symbol name"
