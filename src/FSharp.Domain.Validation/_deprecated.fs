namespace FSharp

[<System.Obsolete("Use FSharp.Domain.Validation instead.")>]
module ValidationBlocks =

    open FSharp.Domain.Validation

    [<System.Obsolete("Use FSharp.Domain.Validation.Box instead.")>]
    module Block =
    
        /// This is the primary way to get a value out of a block
        let value = Box.value

        [<System.Obsolete("Use FSharp.Domain.Validation.Block.``return`` instead.")>]
        let ``return`` = Box.``return``

        [<System.Obsolete("Use FSharp.Domain.Validation.Block.apply instead.")>]
        let apply = Box.apply
    
        // Equality and comparison
        [<System.Obsolete("Use FSharp.Domain.Validation.Block.equals instead.")>]
        let equals = Box.equals
    
        [<System.Obsolete("Use FSharp.Domain.Validation.Block.differs instead.")>]
        let differs = Box.differs

        [<System.Obsolete("Use FSharp.Domain.Validation.Block.compareTo instead.")>]
        let compareTo = Box.compareTo

    [<System.Obsolete("Use FSharp.Domain.Validation.Box instead.")>]
    type Block<'a, 'e> private () = class end with

        /// Creates a block from the given input if valid, otherwise returns an Error (strings are NOT canonicalized),
        /// use Block.verbatim when return type can be inferred, otherwise Block.verbatim<'block>
        static member verbatim<'block when 'block :> IBox<'a,'e>> (inp:'a) : Result<'block, 'e list> =
            Box<'a, 'e>.verbatim<'block> inp

        /// Main function to create boxes, creates a block from the given input if valid,
        /// otherwise returns an Error, use Block.validate when return type can be
        /// inferred, otherwise Block.validate<'block>
        static member validate (inp:'a) : Result<'block, 'e list> =
            Box<'a, 'e>.validate inp

    module Runtime =

        /// Non-generic version of Block.verbatim, mostly meant for serialization
        let verbatim = Runtime.verbatim
        
    type Unchecked<'a> private () = class end with

        /// Creates a block from the given input if valid, otherwise throws an exception,
        /// use Unchecked.blockof when return type can be inferred, otherwise Unchecked.blockof<'block>
        static member blockof<'block when 'block :> IBoxOf<'a>> (inp:'a) : 'block =
            FSharp.Domain.Validation.Unchecked<'a>.boxof<'block> inp


    [<AutoOpen>]
    [<System.Obsolete("Use FSharp.Domain.Validation.NamespaceOperators instead.")>]
    module NamespaceOperators =

        /// Same as Block.value
        let inline (~%) block = FSharp.Domain.Validation.NamespaceOperators.(~%) block

    [<System.Obsolete("Use FSharp.Domain.Validation.Operators instead.")>]
    module Operators = 

        open FSharp.Domain.Validation.Operators
    
        /// Turns predicates into errors, useful when there's a predicate available to use.
        /// Use: String.IsNullOrWhitespace ==> MyError
        [<System.Obsolete("Use FSharp.Domain.Validation.Operators instead.")>]
        let (==>) = (==>)

        /// Turns conditions into errors. Better for conditions made of lambdas.
        /// Use: fun s -> s.Length > 280 => MyError
        [<System.Obsolete("Use FSharp.Domain.Validation.Operators instead.")>]
        let (=>) = (=>)
    
        /// Flattens a list of error lists. Use: fun s -> !? [ condition1 s => MyError; condition2 s => MyError; ]
        [<System.Obsolete("Use FSharp.Domain.Validation.Operators instead.")>]
        let (!?) = (!?)

    [<System.Obsolete("Use FSharp.Domain.Validation.Serialization instead.")>]
    module Serialization =

        [<System.Obsolete("Use FSharp.Domain.Validation.Serialization.ValidationJsonConverterFactory instead.")>]
        type ValidationBlockJsonConverterFactory =
            FSharp.Domain.Validation.Serialization.ValidationJsonConverterFactory