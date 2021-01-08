namespace FSharp.ValidationBlocks



#if FABLE_COMPILER
[<System.Obsolete("For Fable projects use FSharp.ValidationBlocks.Fable instead of FSharp.ValidationBlocks.")>]
#endif
/// A runtime result containing either a valid block or a list of errors
type IBlockResult =

    /// Get the result's block if valid, otherwise throw exception
    abstract member Unbox : unit -> 'result



#if FABLE_COMPILER
[<System.Obsolete("For Fable projects use FSharp.ValidationBlocks.Fable instead of FSharp.ValidationBlocks.")>]
type internal Result<'error>(value:obj, errors:'error list) =
    do failwith "For Fable projects use FSharp.ValidationBlocks.Fable instead of FSharp.ValidationBlocks."
#else
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

    member x.ToResult<'block> () =
        if x.IsOk then x.Value :?> 'block |> Ok
        else Error x.Errors

    interface IBlockResult with
        member x.Unbox () = x.Value |> box |> unbox

#endif