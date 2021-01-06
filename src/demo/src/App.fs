module App

open FSharp.ValidationBlocks.Fable
open FSharp.ValidationBlocks.Fable.Thoth
//open type FSharp.ValidationBlocks.Fable.Block<string, TextError>
open Browser.Dom
open Thoth.Json

// Get bindings for the text area and 3 result contains
let box = document.querySelector("#inp") :?> Browser.Types.HTMLTextAreaElement
let result1 = document.querySelector("#resultBox1") :?> Browser.Types.HTMLSpanElement
let result2 = document.querySelector("#resultBox2") :?> Browser.Types.HTMLSpanElement
let result3 = document.querySelector("#resultBox3") :?> Browser.Types.HTMLSpanElement

let myExtraCoders =

    Extra.empty
    |> Extra.withCustom Codec.Encode<TextBlock> Codec.Decode<TextBlock>
    |> Extra.withCustom Codec.Encode<FreeText> Codec.Decode<FreeText>

// Register our listener
box.oninput <- fun _ ->

    result1.textContent <-
        Block.validate<Text> box.value
        |> Result.text

    result2.textContent <-
        Block.validate<FreeText> box.value
        |> Result.text

    result3.textContent <-
        Block.validate<Integer> box.value
        |> Result.text 


// Enable sandbox if debug
#if DEBUG
    Examples.sandbox ()
#endif