module App

open FSharp.ValidationBlocks.Fable
open Browser.Dom

// Get a reference to our button and cast the Element to an HTMLButtonElement
let myButton = document.querySelector(".my-button") :?> Browser.Types.HTMLButtonElement

// Do the same for the textarea and textboxes
let input = document.querySelector("#inp") :?> Browser.Types.HTMLButtonElement
let textResult = document.querySelector("#textResultBox") :?> Browser.Types.HTMLInputElement
let freeTextResult = document.querySelector("#freeTextResultBox") :?> Browser.Types.HTMLInputElement

// Register our listener
myButton.onclick <- fun _ ->

    textResult.value <-
        Block.validate<Text> input.value |> displayResult

    freeTextResult.value <-
        Block.validate<FreeText> input.value |> displayResult