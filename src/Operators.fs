namespace FSharp.ValidationBlocks

open FSharp.ValidationBlocks.Block

[<AutoOpen>]
module NamespaceOperators =

    /// Same as Block.value
    let inline (~%) block = value block

module Operators = 
    
    /// Turns predicates into errors, useful when there's a predicate available to use.
    /// Use: String.IsNullOrWhitespace ==> MyError
    let (==>) (condition:'a -> bool) (error:'e) (inp:'a) =
        if condition inp then [error] else []

    /// Turns conditions into errors. Better for conditions made of lambdas.
    /// Use: fun s -> s.Length > 280 => MyError
    let (=>) (condition:bool) (error:'e) =
        if condition then [error] else []
    
    /// Flattens a list of error lists. Use: fun s -> !? [ condition1 s => MyError; condition2 s => MyError; ]
    let (!?) (errors:'e list list) : 'e list = errors |> List.concat


