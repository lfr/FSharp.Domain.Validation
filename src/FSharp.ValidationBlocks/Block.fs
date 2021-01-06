namespace FSharp.ValidationBlocks

open Microsoft.FSharp.Reflection
open FSharp.ValidationBlocks.Reflection

#if FABLE_COMPILER
[<System.Obsolete("For Fable projects use FSharp.ValidationBlocks.Fable instead of FSharp.ValidationBlocks.")>]
#endif
module Block =

    #if FABLE_COMPILER
    do failwith "For Fable projects use FSharp.ValidationBlocks.Fable instead of FSharp.ValidationBlocks."
    #endif

    type Module = interface end    
    
    // This enables invoking instance methods on null instances
    let private genericDelegate =
        typedefof<System.Func<_>>.MakeGenericType([|typeof<obj>|])        
    let private nullDelegate t =
        let mi = (Reflection.blockInfo t).ValidateMethod
        System.Delegate
            .CreateDelegate(genericDelegate, null, mi).DynamicInvoke()
        |> fun x -> x, x.GetType().GetMethod("Invoke")


    /// Extracts a value from the block
    let rec internal unwrap (x:obj) =

        let typeError = sprintf "'%A'" x |> sprintf "%s is not a %s"
        
        match FSharpType.GetUnionCases (x.GetType(), true) with
        | [|uci|] ->
            match uci.GetFields() with
            | [|pi|] when FSharpType.IsUnion (pi.PropertyType, true) |> not -> pi.GetValue(x)
            | [|pi|] -> pi.GetValue(x) |> unwrap
            | _ -> "single-case single-field union" |> typeError |> failwith
        | _ -> "single-case union" |> typeError |> failwith


    /// Tail recursive validation of the top block along with the other blocks it's built upon
    let rec internal wrap<'e>
        (src:obj, rollingCtor:UnionCaseInfo list)
        (blockType:System.Type) : Result<'e> =

        let isPrimitive (pi:System.Reflection.PropertyInfo) =
            pi.PropertyType |> typeof<IBlock>.IsAssignableFrom |> not

        let validate (value:obj) blockType =
            let mi, nullDelegate = nullDelegate blockType
            nullDelegate.Invoke(mi, [|value|]) :?> 'e list

        let folder (state:Result<'e>) (uci:UnionCaseInfo) =
            if state.IsOk then
                let newValue = FSharpValue.MakeUnion(uci, [|state.Value|], true)
                let errors = validate src uci.DeclaringType
                Result<'e>(newValue, errors)
            else state

        match FSharpType.GetUnionCases (blockType, true) with
        | [|uci|] ->
            match uci.GetFields() with

            | [|pi|] when pi |> isPrimitive ->
                uci::rollingCtor
                |> List.fold folder (Result<'e>(src, []))

            | [|pi|] ->
                wrap<'e> (src, uci :: rollingCtor) pi.PropertyType

            | x -> sprintf "Expected a single-field union case, but %s contains %i fields." uci.Name x.Length |> failwith
        | x -> sprintf "Expected a single-case union, but %s contains %i cases." blockType.Name x.Length |> failwith


    /// Internal validation method, just a wrapper with some type checks, not meant to be used outside of this library
    let internal validateInternal<'block, 'a, 'err when 'block :> IBlock<'a,'err>> (src:'a) : Result<'block, 'err list> =

        let t = typeof<'block>

        // ideally this would be a type constraint on 'block, but sum type constrains don't exist yet
        let typeConstraintError =
            sprintf "%s, more anotations may help determine more accurately this method's return type." >> failwith
        if not<|FSharpType.IsUnion(t, true) then
            t.Name |> sprintf "%s is not a Discriminated Union" |> typeConstraintError
        elif FSharpType.GetUnionCases(t, Flags.NonPublic ||| Flags.Instance).Length <> 1 then
            t.Name |> sprintf "%s is not a single-case signe-value union with private constructor" |> typeConstraintError

        //let result = src |> wrap<'error> t
        let result = wrap<'err> (src, []) t

        result.ToResult<'block>()
    
    /// This is the primary way to get a value out of a block
    let rec value (src:IBlock<'baseType,'error>) =
        src |> unwrap :?> 'baseType


    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let ``return``<'baseType, 'error> (inp:'baseType) : IBlock<'baseType, 'error> =
        NoValidation<'baseType, 'error>.Block inp :> _

    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let apply (f:IBlock<'baseType -> 'a, 'error>) (x:IBlock<'baseType,'error>) =
        value x |> value f |> ``return``<'a, 'error>    
    
    // Equality and comparison
    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let equals<'baseType, 'e when 'baseType : equality>
        (left:IBlock<'baseType, 'e>) (right:IBlock<'baseType, 'e>) =
        value left = value right
    
    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let differs t1 t2 = equals t1 t2 |> not

    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let compareTo<'baseType, 'e when 'baseType :> System.IComparable<'baseType>>
        (left:IBlock<'baseType, 'e>) (right:IBlock<'baseType, 'e>) =
        (value left).CompareTo(value right)

type Block<'a, 'e> private () = class end with

    /// Creates a block from the given input if valid, otherwise returns an Error (strings are NOT canonicalized),
    /// use Block.verbatim when return type can be inferred, otherwise Block.verbatim<'block>
    static member verbatim<'block when 'block :> IBlock<'a,'e>> (inp:'a) : Result<'block, 'e list> =
        Block.validateInternal<'block, 'a, 'e> inp

    /// Main function to create blocks, creates a block from the given input if valid,
    /// otherwise returns an Error, use Block.validate when return type can be
    /// inferred, otherwise Block.validate<'block>
    static member validate (inp:'a) : Result<'block, 'e list> =
        Utils.canonicalize inp typeof<'a> |> Block<'a, 'e>.verbatim


module Runtime =

    let ``nameof Block.wrap`` = "wrap"
    
    let private wrapMi =
        (``nameof Block.wrap``, Flags.NonPublic ||| Flags.Static)
        |> typeof<Block.Module>.DeclaringType.GetMethod

    /// Non-generic version of Block.verbatim, mostly meant for serialization
    let verbatim (blockType:System.Type) (input:obj) : IBlockResult =
        let bi = Reflection.blockInfo blockType
        let mi = wrapMi.MakeGenericMethod([|bi.ErrorType|])
        mi.Invoke(null, [|input; List.empty<UnionCaseInfo>; blockType|]) :?> _
        

type Unchecked<'a> private () = class end with

    /// Creates a block from the given input if valid, otherwise throws an exception,
    /// use Unchecked.blockof when return type can be inferred, otherwise Unchecked.blockof<'block>
    static member blockof<'block when 'block :> IBlockOf<'a>> (inp:'a) : 'block =
        let result = Runtime.verbatim typeof<'block> inp
        result.Unbox()