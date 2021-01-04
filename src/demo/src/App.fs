module App

open FSharp.ValidationBlocks.Fable
open Browser.Dom

// Get bindings for the text area and 3 result contains
let input = document.querySelector("#inp") :?> Browser.Types.HTMLTextAreaElement
let result1 = document.querySelector("#resultBox1") :?> Browser.Types.HTMLSpanElement
let result2 = document.querySelector("#resultBox2") :?> Browser.Types.HTMLSpanElement
let result3 = document.querySelector("#resultBox3") :?> Browser.Types.HTMLSpanElement

// Register our listener
input.oninput <- fun _ ->

    result1.textContent <-
        Block.validate<Text> input.value
        |> Result.text

    result2.textContent <-
        Block.validate<FreeText> input.value
        |> Result.text

    result3.textContent <-
        Block.validate<Integer> input.value
        |> Result.text