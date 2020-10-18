namespace AstTransformations

module Helpers =
    let flip: ('a -> 'b -> 'c) -> 'b -> 'a -> 'c =
        fun f a b -> f b a
