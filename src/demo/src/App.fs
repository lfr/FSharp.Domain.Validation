module App


open type FSharp.ValidationBlocks.Fable.Block<string, TextError>
open Browser.Dom

// Get bindings for the text area and 3 result contains
let inp = document.querySelector("#inp") :?> Browser.Types.HTMLTextAreaElement
let result1 = document.querySelector("#resultBox1") :?> Browser.Types.HTMLSpanElement
let result2 = document.querySelector("#resultBox2") :?> Browser.Types.HTMLSpanElement
let result3 = document.querySelector("#resultBox3") :?> Browser.Types.HTMLSpanElement

// Register our listener
inp.oninput <- fun _ ->

    result1.textContent <-
        validate<Text> inp.value
        |> Result.toText

    result2.textContent <-
        validate<FreeText> inp.value
        |> Result.toText

    result3.textContent <-
        validate<Integer> inp.value
        |> Result.toText 



// Enable sandbox in dev server
#if DEBUG
    Examples.sandbox ()
#endif