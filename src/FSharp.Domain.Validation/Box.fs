namespace FSharp.Domain.Validation

open Microsoft.FSharp.Reflection
open FSharp.Domain.Validation.Reflection

module Box =

    type Module = interface end    
    
    // This enables invoking instance methods on null instances
    let private genericDelegate =
        typedefof<System.Func<_>>.MakeGenericType([|typeof<obj>|])        
    let private nullDelegate t =
        let mi = (Reflection.boxinfo t).ValidateMethod
        System.Delegate
            .CreateDelegate(genericDelegate, null, mi).DynamicInvoke()
        |> fun x -> x, x.GetType().GetMethod("Invoke")


    /// Extracts a value from the box
    let rec internal unwrap (x:obj) =

        let typeError = sprintf "'%A'" x |> sprintf "%s is not a %s"
        
        match FSharpType.GetUnionCases (x.GetType(), true) with
        | [|uci|] ->
            match uci.GetFields() with
            | [|pi|] when FSharpType.IsUnion (pi.PropertyType, true) |> not -> pi.GetValue(x)
            | [|pi|] -> pi.GetValue(x) |> unwrap
            | _ -> "single-case single-field union" |> typeError |> failwith
        | _ -> "single-case union" |> typeError |> failwith


    /// Tail recursive validation of the top box along with the other boxes it's built upon
    let rec internal wrap<'e>
        (src:obj, rollingCtor:UnionCaseInfo list)
        (boxType:System.Type) : Result<'e> =

        let isPrimitive (pi:System.Reflection.PropertyInfo) =
            pi.PropertyType |> typeof<IBox>.IsAssignableFrom |> not

        let validate (value:obj) boxType =
            let mi, nullDelegate = nullDelegate boxType
            nullDelegate.Invoke(mi, [|value|]) :?> 'e list

        let folder (state:Result<'e>) (uci:UnionCaseInfo) =
            if state.IsOk then
                let newValue = FSharpValue.MakeUnion(uci, [|state.Value|], true)
                let errors = validate src uci.DeclaringType
                Result<'e>(newValue, errors)
            else state

        match FSharpType.GetUnionCases (boxType, true) with
        | [|uci|] ->
            match uci.GetFields() with

            | [|pi|] when pi |> isPrimitive ->
                uci::rollingCtor
                |> List.fold folder (Result<'e>(src, []))

            | [|pi|] ->
                wrap<'e> (src, uci :: rollingCtor) pi.PropertyType

            | x -> sprintf "Expected a single-field union case, but %s contains %i fields." uci.Name x.Length |> failwith
        | x -> sprintf "Expected a single-case union, but %s contains %i cases." boxType.Name x.Length |> failwith


    /// Internal validation method, just a wrapper with some type checks, not meant to be used outside of this library
    let internal validateInternal<'box, 'a, 'err when 'box :> IBox<'a,'err>> (src:'a) : Result<'box, 'err list> =

        let t = typeof<'box>

        // ideally this would be a type constraint on 'box, but sum type constrains don't exist yet
        let typeConstraintError =
            sprintf "%s, more anotations may help determine more accurately this method's return type." >> failwith
        if not<|FSharpType.IsUnion(t, true) then
            t.Name |> sprintf "%s is not a Discriminated Union" |> typeConstraintError
        elif FSharpType.GetUnionCases(t, Flags.NonPublic ||| Flags.Instance).Length <> 1 then
            t.Name |> sprintf "%s is not a single-case signe-value union with private constructor" |> typeConstraintError

        //let result = src |> wrap<'error> t
        let result = wrap<'err> (src, []) t

        result.ToResult<'box>()
    
    /// This is the primary way to get a value out of a box
    let rec value (src:IBox<'baseType,'error>) =
        src |> unwrap :?> 'baseType


    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let ``return``<'baseType, 'error> (inp:'baseType) : IBox<'baseType, 'error> =
        NoValidation<'baseType, 'error>.Box inp :> _

    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let apply (f:IBox<'baseType -> 'a, 'error>) (x:IBox<'baseType,'error>) =
        value x |> value f |> ``return``<'a, 'error>    
    
    // Equality and comparison
    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let equals<'baseType, 'e when 'baseType : equality>
        (left:IBox<'baseType, 'e>) (right:IBox<'baseType, 'e>) =
        value left = value right
    
    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let differs t1 t2 = equals t1 t2 |> not

    [<System.Obsolete("This function's existence and final signature are still being investigated.")>]
    let compareTo<'baseType, 'e when 'baseType :> System.IComparable<'baseType>>
        (left:IBox<'baseType, 'e>) (right:IBox<'baseType, 'e>) =
        (value left).CompareTo(value right)

type Box<'a, 'e> private () = class end with

    /// Creates a box from the given input if valid, otherwise returns an Error (strings are NOT canonicalized),
    /// use Box.verbatim when return type can be inferred, otherwise Box.verbatim<'box>
    static member verbatim<'box when 'box :> IBox<'a,'e>> (inp:'a) : Result<'box, 'e list> =
        Box.validateInternal<'box, 'a, 'e> inp

    /// Main function to create boxes, creates a box from the given input if valid,
    /// otherwise returns an Error, use Box.validate when return type can be
    /// inferred, otherwise Box.validate<'box>
    static member validate (inp:'a) : Result<'box, 'e list> =
        Utils.canonicalize inp typeof<'a> |> Box<'a, 'e>.verbatim

module Runtime =
    
    let private wrapMi =
        (nameof Box.wrap, Flags.NonPublic ||| Flags.Static)
        |> typeof<Box.Module>.DeclaringType.GetMethod

    /// Non-generic version of Box.verbatim, mostly meant for serialization
    let verbatim (boxType:System.Type) (input:obj) : IBoxResult =
        let bi = Reflection.boxinfo boxType
        let mi = wrapMi.MakeGenericMethod([|bi.ErrorType|])
        mi.Invoke(null, [|input; List.empty<UnionCaseInfo>; boxType|]) :?> _

type Unchecked<'a> private () = class end with

    /// Creates a box from the given input if valid, otherwise throws an exception,
    /// use Unchecked.boxof when return type can be inferred, otherwise Unchecked.boxof<'box>
    static member boxof<'box when 'box :> IBoxOf<'a>> (inp:'a) : 'box =
        let result = Runtime.verbatim typeof<'box> inp
        result.Unbox()