namespace FSharp.ValidationBlocks.Thoth

open FSharp.ValidationBlocks
open Thoth.Json

/// Contains custom encoder and decoder to property serialize blocks as their
/// inner type, use both encoder and decoder together for each block type:
/// Extra.empty |> Extra.withCustome Codec.Encode<your-block-type> Codec.Decode<your-block-type>
type Codec<'a, 'e> private () = class end with

    /// Custom encoder for validation blocks to be properly serialized in json
    /// as their inner type, use along with decoder:
    /// Extra.empty |> Extra.withCustom Codec.Encode<your-block-type> Codec.Decode<your-block-type>
    static member inline Encode<'block when 'block :> IBlock<'a,'e>> (block:'block) : JsonValue =
         Encode.Auto.toString(4, Block.value block) :> _

    /// Custom decoder for validation blocks to be properly serialized in json
    /// as their inner type, use along with encoder:
    /// Extra.empty |> Extra.withCustom Codec.Encode<your-block-type> Codec.Decode<your-block-type>
    static member inline Decode<'block when 'block :> IBlock<'a,'e>> (path:string) (value:JsonValue) : Result<'block, DecoderError> =
        Decode.Auto.fromString<'a>(value :?> string)
        |> Result.map (fun x -> box x :?> 'a)
        |> function
        | Ok x ->
            Block.validate x
            |> Result.mapError
                (fun e -> path, e |> List.map (sprintf "%A") |> BadOneOf)
        | Error e -> Error (path, FailMessage e)