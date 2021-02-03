namespace FSharp.ValidationBlocks

[<System.Obsolete("The Fable-specific namespace no longer exist. Use FSharp.Domain.Validation instead.")>]
module Fable =

    type Type = System.Type

    [<System.Obsolete("Use FSharp.Domain.Validation.IBox instead.")>]
    type IBlock = FSharp.ValidationBlocks.IBlock
    [<System.Obsolete("Use FSharp.Domain.Validation.IBox instead.")>]
    type IBlock<'baseType, 'error> = FSharp.ValidationBlocks.IBlock<'baseType, 'error> 
    [<System.Obsolete("Use FSharp.Domain.Validation.IBoxOf instead.")>]
    type IBlockOf<'baseType> = FSharp.ValidationBlocks.IBlockOf<'baseType>

    open FSharp.Domain.Validation

    [<System.Obsolete("Use FSharp.Domain.Validation.Box instead.")>]
    module Block =

        /// Returns true if an object is a block
        [<System.Obsolete("Use FSharp.Domain.Validation.Box.isBlock instead.")>]
        let isBlock (inp:obj) = Box.isBox inp

        /// This is the primary way to get a value out of a box
        [<System.Obsolete("Use FSharp.Domain.Validation.Box.value instead.")>]
        let value (src:'block when 'block :> IBlock<'baseType,'error>) : 'baseType =
            Box.value src

    [<System.Obsolete("Use FSharp.Domain.Validation.Box instead.")>]
    type Block<'a, 'e> private () = class end with

        /// Creates a block from the given input if valid, otherwise returns an Error (strings are NOT canonicalized),
        /// use Block.verbatim when return type can be inferred, otherwise Block.verbatim<'block>
        [<System.Obsolete("Use FSharp.Domain.Validation.Box.verbatim instead.")>]
        static member inline verbatim<'block when 'block :> IBlock<'a,'e>> (inp:'a) : Result<'block, 'e list> =
            Box<'a, 'e>.verbatim<'block> inp

        /// Main function to create boxes, creates a block from the given input if valid,
        /// otherwise returns an Error, use Block.validate when return type can be
        /// inferred, otherwise Block.validate<'block>
        [<System.Obsolete("Use FSharp.Domain.Validation.Box.validate instead.")>]
        static member inline validate<'block when 'block :> IBlock<'a,'e>> (inp:'a) : Result<'block, 'e list> =
            Box<'a, 'e>.validate<'block> inp

    [<AutoOpen>]
    [<System.Obsolete("Use FSharp.Domain.Validation.NamespaceOperators instead.")>]
    module NamespaceOperators =

        /// Same as Block.value
        let inline (~%) block = FSharp.Domain.Validation.NamespaceOperators.(~%) block

    [<System.Obsolete("Use FSharp.Domain.Validation.Operators instead.")>]
    module Operators = 
    
        /// Turns predicates into errors, useful when there's a predicate available to use.
        /// Use: String.IsNullOrWhitespace ==> MyError
        [<System.Obsolete("Use FSharp.Domain.Validation.Operators instead.")>]
        let (==>) = FSharp.Domain.Validation.Operators.(==>)

        /// Turns conditions into errors. Better for conditions made of lambdas.
        /// Use: fun s -> s.Length > 280 => MyError
        [<System.Obsolete("Use FSharp.Domain.Validation.Operators instead.")>]
        let (=>) = FSharp.Domain.Validation.Operators.(=>)
    
        /// Flattens a list of error lists. Use: fun s -> !? [ condition1 s => MyError; condition2 s => MyError; ]
        [<System.Obsolete("Use FSharp.Domain.Validation.Operators instead.")>]
        let (!?) = FSharp.Domain.Validation.Operators.(!?)

    module Thoth =

        [<System.Obsolete("Use FSharp.Domain.Validation.Thoth.Codec instead.")>]
        type Codec<'a, 'e> = FSharp.Domain.Validation.Thoth.Codec<'a, 'e>