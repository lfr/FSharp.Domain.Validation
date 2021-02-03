namespace FSharp.Domain.Validation.Thoth

open FSharp.Domain.Validation
open Thoth.Json

/// Contains custom encoder and decoder to property serialize boxes as their
/// inner type, use both encoder and decoder together for each box type:
/// Extra.empty |> Extra.withCustome Codec.Encode<your-box-type> Codec.Decode<your-box-type>
type Codec<'a, 'e> private () = class end with

    /// Custom encoder for validation boxes to be properly serialized in json
    /// as their inner type, use along with decoder:
    /// Extra.empty |> Extra.withCustom Codec.Encode<your-box-type> Codec.Decode<your-box-type>
    static member inline Encode<'box when 'box :> IBox<'a,'e>> (box:'box) : JsonValue =
         Encode.Auto.toString(4, Box.value box) :> _

    /// Custom decoder for validation boxes to be properly serialized in json
    /// as their inner type, use along with encoder:
    /// Extra.empty |> Extra.withCustom Codec.Encode<your-box-type> Codec.Decode<your-box-type>
    static member inline Decode<'box when 'box :> IBox<'a,'e>> (path:string) (value:JsonValue) : Result<'box, DecoderError> =
        Decode.Auto.fromString<'a>(value :?> string)
        |> Result.map (fun x -> box x :?> 'a)
        |> function
        | Ok x ->
            Box.validate x
            |> Result.mapError
                (fun e -> path, e |> List.map (sprintf "%A") |> BadOneOf)
        | Error e -> Error (path, FailMessage e)