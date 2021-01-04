module App

open FSharp.ValidationBlocks.Fable
open type FSharp.ValidationBlocks.Fable.Block<string, TextError>
open Browser.Dom

// Get bindings for the text area and 3 result contains
let box = document.querySelector("#inp") :?> Browser.Types.HTMLTextAreaElement
let result1 = document.querySelector("#resultBox1") :?> Browser.Types.HTMLSpanElement
let result2 = document.querySelector("#resultBox2") :?> Browser.Types.HTMLSpanElement
let result3 = document.querySelector("#resultBox3") :?> Browser.Types.HTMLSpanElement

// Register our listener
box.oninput <- fun _ ->

    result1.textContent <-
        validate<Text> box.value
        |> Result.text

    result2.textContent <-
        validate<FreeText> box.value
        |> Result.text

    result3.textContent <-
        validate<Integer> box.value
        |> Result.text