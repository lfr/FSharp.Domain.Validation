﻿module Examples

open Browser.Dom
open Thoth.Json
open FSharp.Domain.Validation
open FSharp.Domain.Validation.Thoth

let pre = document.querySelector("pre") :?> Browser.Types.HTMLPreElement
let box = document.querySelector("#inp") :?> Browser.Types.HTMLTextAreaElement

let sandbox () =
    
    let thothCustomEncoders =
        Extra.empty
        |> Extra.withCustom Codec.Encode<FreeText> Codec.Decode<FreeText>
    
    pre.textContent <-
        let value =
            {|
                Block = typeof<FreeText>.Name
                Value = 
                    Box.validate<FreeText> box.value
                    |> function Ok x -> x | _ -> failwith "invalid"
            |}

        sprintf "Toth serialized content:\n%s" <| 
        Encode.Auto.toString (4, value, extra = thothCustomEncoders)
