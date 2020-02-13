namespace FSharp.ValidationBlocks

open Microsoft.FSharp.Reflection
type Flags = System.Reflection.BindingFlags

module Block =

    type Module = interface end

    // The point of this code is to allow calling an instance method on a null instance
    // This only works on instance methods that do not actually use the instance itself
    let private nullDelegate (t:System.Type) =
        let validate = //nameof IBlock<_,_>.Validation doesn't work
            "Validate"
        let mi = t.GetMethods(Flags.NonPublic ||| Flags.Instance) |> Array.find (fun mi -> mi.Name.EndsWith validate)
        System.Delegate.CreateDelegate(typedefof<System.Func<_>>.MakeGenericType([|typeof<obj>|]), null, mi).DynamicInvoke()
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

    /// Tail recursive validation of the top block and all other blocks its built upon
    let rec private wrap<'e>
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


    /// Non-generic version of wrap to facilitate serialization
    let private wrapMi = typeof<Module>.DeclaringType.GetMethod(nameof wrap, Flags.NonPublic ||| Flags.Static)
    let internal runtimeWrap (blockType:System.Type) (errorType:System.Type) (src:obj) : IResult =            
        let mi = wrapMi.MakeGenericMethod([|errorType|])
        mi.Invoke(null, [|src; List.empty<UnionCaseInfo>; blockType|]) :?> _


    /// This is the primary way to create blocks out of their base (primitive) types
    let validate<'block, 'a, 'error when 'block :> IBlock<'a, 'error>> (src:'a) : Result<'block,'error list> =

        let t = typeof<'block>

        // ideally this would be a type constraint on 'block, but sum type constrains don't exist yet
        let typeConstraintError =
            sprintf "%s, more anotations may help determine more accurately this method's return type." >> failwith
        if not<|FSharpType.IsUnion(t, true) then
            t.Name |> sprintf "%s is not a Discriminated Union" |> typeConstraintError
        elif FSharpType.GetUnionCases(t, Flags.NonPublic ||| Flags.Instance).Length <> 1 then
            t.Name |> sprintf "%s is not a single-case signe-value union with private constructor" |> typeConstraintError

        //let result = src |> wrap<'error> t
        let result = wrap<'error> (src, []) t

        result.ToResult<'block>()

    let ofUnchecked<'block, 'a, 'error when 'block :> IBlock<'a, 'error>> src =
        match validate src with
        | Ok block -> block
        | Error e ->
            sprintf "Block validation failed with error: %A" e |> failwith
    
    /// This is the primary way to get a value out of a block
    let rec value (src:IBlock<'baseType,'error>) =
        src |> unwrap :?> 'baseType

    /// Same as Block.value
    let inline (~%) block = value block


    [<System.Obsolete("This function's potential need and final signature is still being investigated.")>]
    let ``return``<'baseType, 'error> (inp:'baseType) : IBlock<'baseType, 'error> =
        NoValidation<'baseType, 'error>.Block inp :> _

    [<System.Obsolete("This function's potential need and final signature is still being investigated.")>]
    let apply (f:IBlock<'baseType -> 'a, 'error>) (x:IBlock<'baseType,'error>) =
        value x |> value f |> ``return``<'a, 'error>

    
    
    // Equality and comparison
    [<System.Obsolete("This function's potential need and final signature is still being investigated.")>]
    let equals<'baseType, 'e when 'baseType : equality>
        (left:IBlock<'baseType, 'e>) (right:IBlock<'baseType, 'e>) =
        %left = %right
    
    [<System.Obsolete("This function's potential need and final signature is still being investigated.")>]
    let differs t1 t2 = equals t1 t2 |> not

    [<System.Obsolete("This function's potential need and final signature is still being investigated.")>]
    let compareTo<'baseType, 'e when 'baseType :> System.IComparable<'baseType>>
        (left:IBlock<'baseType, 'e>) (right:IBlock<'baseType, 'e>) =
        (%left).CompareTo(%right)


module Operators =

    let (=>) (condition:'a -> bool) (error:'e) (inp:'a) =
        if condition inp then [error] else []

    let (!?) (errorConditions:('a -> 'e list) list) (inp:'a) : 'e list =
        errorConditions |> List.collect (fun f -> f inp)