namespace FSharp.Domain.Validation

open Fable.Core.JsInterop
open Microsoft.FSharp.Reflection
open FSharp.Domain.Validation
open System.Reflection

type Type = System.Type

// we'll use this marker to get the namespace
type __ = interface end

module Box =

    // we'll use this to safely get the name of the validate method
    let private __ = {new IBox<_,_> with member _.Validate = fun _ -> []}

    // TODO: get rid of this as soon as proper nameof is supported
    [<System.Obsolete("This is for internal use.")>]
    module _nameof =
        let Validate = "Validate"
        let wrap = "wrap"
        let IBox = typedefof<IBox<_,_>>.Name

    /// Returns true if an object is a box
    let isBox (inp:obj) = inp?(_nameof.Validate) <> None

    /// This is the primary way to get a value out of a box
    let rec value (src:'box when 'box :> IBox<'baseType,'error>) : 'baseType =

        let typeError s = sprintf "%A is not a %s" src s
        //let typeError s = $"%A{src$ is not a %s{s}" ← replace above with this in net5.0

        match src?fields with

        // union case has only one field that has no inner fields
        | Some x when x?(1) = None && not<|isBox x?(0) -> x?(0) |> unbox

        // union case has only one field that does have inner fields
        | Some x when x?(1) = None -> x?(0) |> value

        // union case has more than one field
        | Some _ -> "single-case single-field union" |> typeError |> failwith

        // union case has no fields
        | _ -> "single-case union" |> typeError |> failwith

type Box<'a, 'e> private () = class end with

    [<System.Obsolete("Do not call this method, it's intended for internal use.")>]
    // this method cannot be made private because the calling method has to be inline
    static member wrap (originalInput:'a) (boxType:Type) =

        match FSharpType.GetUnionCases (boxType, true) with
        | [|uci|] ->
            match uci.GetFields() with

            | [|pi|] when not<|FSharpType.IsUnion pi.PropertyType ->
                let primitiveBox =
                    FSharpValue.MakeUnion(uci, [|box originalInput|], true)
                    :?> IBox<'a, 'e>
                primitiveBox.Validate originalInput, primitiveBox

            | [|pi|] ->
                let errors, innerBox =
                    Box<'a,'e>.wrap originalInput pi.PropertyType

                let box =
                    FSharpValue.MakeUnion(uci, [|box innerBox|], true)
                    :?> IBox<'a, 'e>

                match errors with
                | [] -> box.Validate originalInput, innerBox
                | e -> e, innerBox

            | x -> sprintf "Expected a single-field union case, but %s contains %i fields." uci.Name x.Length |> failwith
        | x -> sprintf "Expected a single-case union, but %s contains %i cases." boxType.Name x.Length |> failwith

    /// Creates a box from the given input if valid, otherwise returns an Error (strings are NOT canonicalized),
    /// use Box.verbatim when return type can be inferred, otherwise Box.verbatim<'box>
    static member inline verbatim<'box when 'box :> IBox<'a,'e>> (inp:'a) : Result<'box, 'e list> =

        if FSharpType.IsUnion typeof<'a> then
            typeof<__>.Namespace
            |> failwithf "%s doesn't currently support boxes of union types (except for Option<'a>)"
        
        match Box<'a,'e>.wrap inp typeof<'box> with
        | [], box -> box :?> _ |> Ok
        | errors, _ -> Error errors

    /// Main function to create boxes, creates a box from the given input if valid,
    /// otherwise returns an Error, use Box.validate when return type can be
    /// inferred, otherwise Box.validate<'box>
    static member inline validate<'box when 'box :> IBox<'a,'e>> (inp:'a) : Result<'box, 'e list> =
        FSharp.Domain.Validation.Utils.canonicalize
            inp typeof<'a> |> Box<'a,'e>.verbatim


open Box

type Unchecked<'a> private () = class end with

    /// Creates a box from the given input if valid, otherwise throws an exception,
    /// use Unchecked.Boxof when return type can be inferred, otherwise Unchecked.Boxof<'box>
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
    static member inline boxof<'box when 'box :> IBoxOf<'a>> (inp:'a) =

        let errorType =
            typeof<'box>
                .GetInterface(_nameof.IBox)
                .GetGenericArguments().[1] // 0 is base type, 1 is error type
        
        let wrapMi =
            typedefof<Box<_,_>>
                .MakeGenericType(typeof<'a>, errorType)
                .GetMethod(_nameof.wrap,
                    BindingFlags.Static ||| BindingFlags.Public)

        wrapMi.Invoke(null, [|box inp|])
        :?> (obj list * 'box)
        |> function
        | [], x -> x
        | _ -> failwithf "Cannot create box with invalid input '%A'" inp