namespace FSharp.ValidationBlocks.Fable

open Fable.Core.JsInterop
open Microsoft.FSharp.Reflection
open System.Reflection

type Type = System.Type

// we'll use this marker to get the namespace
type __ = interface end

module Block =

    // we'll use this to safely get the name of the validate method
    let private __ = {new IBlock<_,_> with member _.Validate = fun _ -> []}

    // TODO: get rid of this as soon as proper nameof is supported
    [<System.Obsolete("This is for internal use.")>]
    module _nameof =
        let Validate = "Validate"
        let wrap = "wrap"
        let iblock = typedefof<IBlock<_,_>>.Name

    /// Returns true if an object is a block
    let isBlock (inp:obj) = inp?(_nameof.Validate) <> None

    let rec value (src:'block when 'block :> IBlock<'baseType,'error>) : 'baseType =

        let typeError s = sprintf "%A is not a %s" src s
        //let typeError s = $"%A{src$ is not a %s{s}" ← replace above with this in net5.0

        match src?fields with

        // union case has only one field that has no inner fields
        | Some x when x?(1) = None && not<|isBlock x?(0) -> x?(0) |> unbox

        // union case has only one field that does have inner fields
        | Some x when x?(1) = None -> x?(0) |> value

        // union case has more than one field
        | Some _ -> "single-case single-field union" |> typeError |> failwith

        // union case has no fields
        | _ -> "single-case union" |> typeError |> failwith

type Block<'a, 'e> private () = class end with

    static member wrap (originalInput:'a) (blockType:Type) =

        match FSharpType.GetUnionCases (blockType, true) with
        | [|uci|] ->
            match uci.GetFields() with

            | [|pi|] when not<|FSharpType.IsUnion pi.PropertyType ->
                let primitiveBlock =
                    FSharpValue.MakeUnion(uci, [|box originalInput|], true)
                    :?> IBlock<'a, 'e>
                primitiveBlock.Validate originalInput, primitiveBlock

            | [|pi|] ->
                let errors, innerBlock =
                    Block<'a,'e>.wrap originalInput pi.PropertyType

                let block =
                    FSharpValue.MakeUnion(uci, [|box innerBlock|], true)
                    :?> IBlock<'a, 'e>

                errors @ block.Validate originalInput, innerBlock

            | x -> sprintf "Expected a single-field union case, but %s contains %i fields." uci.Name x.Length |> failwith
        | x -> sprintf "Expected a single-case union, but %s contains %i cases." blockType.Name x.Length |> failwith

    /// Creates a block from the given input if valid, otherwise returns an Error (strings are NOT canonicalized),
    /// use Block.verbatim when return type can be inferred, otherwise Block.verbatim<'block>
    static member inline verbatim<'block when 'block :> IBlock<'a,'e>> (inp:'a) : Result<'block, 'e list> =

        if FSharpType.IsUnion typeof<'a> then
            typeof<__>.Namespace
            |> failwithf "%s doesn't currently support blocks of union types (except for Option<'a>)"
        
        match Block<'a,'e>.wrap inp typeof<'block> with
        | [], block -> block :?> _ |> Ok
        | errors, _ -> Error errors

    /// Main function to create blocks, creates a block from the given input if valid,
    /// otherwise returns an Error, use Block.validate when return type can be
    /// inferred, otherwise Block.validate<'block>
    static member inline validate<'block when 'block :> IBlock<'a,'e>> (inp:'a) : Result<'block, 'e list> =
        FSharp.ValidationBlocks.Utils.canonicalize
            inp typeof<'a> |> Block<'a,'e>.verbatim


open Block

type Unchecked<'a> private () = class end with

    /// Creates a block from the given input if valid, otherwise throws an exception,
    /// use Unchecked.blockof when return type can be inferred, otherwise Unchecked.blockof<'block>
    [<System.Obsolete("This is not currently supported in Fable (issue #2321).", true)>]
    // Missing reflection support:
    // 🔲 Type.GetInterface
    // ✅ Type.MakeGenericType
    // ✅ GetGenericArguments
    // ✅ Type.IsGenericType
    // ✅ Type.GetGenericTypeDefinition
    // 🔲 Type.GetMethods
    // 🔲 MethodBase.Invoke
    // https://github.com/fable-compiler/Fable/issues/2321
    static member inline blockof<'block when 'block :> IBlockOf<'a>> (inp:'a) =

        let errorType =
            typeof<'block>
                .GetInterface(_nameof.iblock)
                .GetGenericArguments().[1] // 0 is base type, 1 is error type
        
        let wrapMi =
            typedefof<Block<_,_>>
                .MakeGenericType(typeof<'a>, errorType)
                .GetMethod(_nameof.wrap,
                    BindingFlags.Static ||| BindingFlags.Public)

        wrapMi.Invoke(null, [|box inp|])
        :?> (obj list * 'block)
        |> function
        | [], x -> x
        | _ -> failwithf "Cannot create block with invalid input '%A'" inp