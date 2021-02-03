namespace FSharp.Domain.Validation



/// A runtime result containing either a valid box or a list of errors
type IBoxResult =

    /// Get the result's box if valid, otherwise throw exception
    abstract member Unbox : unit -> 'result



/// This helper type should not be used outside of this library
type internal Result<'error>(value:obj, errors:'error list) =


    member x.IsOk = errors |> List.isEmpty
    member x.Errors =
        if x.IsOk then
            failwith "Attempt to access errors of valid result." else errors
    member x.Value =
        if x.IsOk then value else
            sprintf "Validation of the given input returned errors: %A" errors
            |> failwith

    member x.ToResult<'box> () =
        if x.IsOk then x.Value :?> 'box |> Ok
        else Error x.Errors

    interface IBoxResult with
        member x.Unbox () = x.Value |> box |> unbox