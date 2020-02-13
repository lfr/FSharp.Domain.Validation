namespace FSharp.ValidationBlocks

/// This helper type is inconsequential outside of this library
type internal IResult =
    abstract member Unbox : unit -> 'result

/// This helper type is inconsequential outside of this library
type internal Result<'error>(value:obj, errors:'error list) =
    member x.IsOk = errors |> List.isEmpty
    member x.Errors =
        if x.IsOk then
            failwith "Attempt to access errors of valid result." else errors
    member x.Value =
        if x.IsOk then value else failwith "Attempt to access error result."

    member x.ToResult<'result> () =
        if x.IsOk then x.Value :?> 'result |> Ok
        else Error x.Errors

    interface IResult with
        member x.Unbox () = x.Value |> box |> unbox


